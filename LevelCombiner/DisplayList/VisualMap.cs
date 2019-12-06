using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LevelCombiner
{
    // List of F3D commands basically
    public class TextureDescription
    {
        protected List<UInt64> cmds = new List<UInt64>();

        public override bool Equals(object obj)
        {
            TextureDescription list = (TextureDescription)(obj);
            return Enumerable.SequenceEqual(cmds, list.cmds);
        }

        static public bool Equals(TextureDescription td1, TextureDescription td2)
        {
            return Enumerable.SequenceEqual(td1.cmds, td2.cmds);
        }

        public override int GetHashCode()
        {
            int hash = 0;
            foreach (UInt64 cmd in cmds)
            {
                hash ^= cmd.GetHashCode();
            }
            return hash;
        }

        public UInt64 GetTextureCMD()
        {
            foreach (UInt64 cmd in cmds)
            {
                if ((cmd & 0xFF00000000000000) == 0xFD00000000000000)
                    return cmd;
            }
            return 0;
        }

        public override string ToString()
        {
            foreach (UInt64 cmd in cmds)
            {
                if ((cmd & 0xFF00000000000000) == 0xfd00000000000000)
                    return cmd.ToString("X8");
            }

            return base.ToString();
        }

        public void Add(UInt64 cmd)
        {
            cmds.Add(cmd);
        }

        public void AddRange(List<UInt64> otherCmds)
        {
            cmds.AddRange(otherCmds);
        }

        public static implicit operator List<UInt64>(TextureDescription td) => td.cmds;
    }

    public class ScrollingTextureDescription : TextureDescription
    {
        public ScrollDesc scroll;

        public ScrollingTextureDescription() : base() 
        {
            scrollRegions = new SortedRegionList();
            vertexRegions = new SortedRegionList();
        }

        public override bool Equals(object obj)
        {
            ScrollingTextureDescription std = (ScrollingTextureDescription) obj;
            if (scroll != null && !scroll.Equals(std.scroll))
                return false;
                
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            int hash = 0;
            foreach (UInt64 cmd in cmds)
            {
                hash ^= cmd.GetHashCode();
            }
            if (scroll != null)
                hash ^= scroll.GetHashCode();

            return hash;
        }

        public override string ToString()
        {
            string str = "???";
            foreach (UInt64 cmd in cmds)
            {
                if ((cmd & 0xFF00000000000000) == 0xFD00000000000000)
                {
                    str = cmd.ToString("X8");
                    break;
                }
            }

            if (scroll != null)
            {
                str += " " + scroll.ToString();
            }

            return str;
        }

        public SortedRegionList scrollRegions;
        public void RegisterScroll(Scroll scr)
        {
            if (scr == null)
                return;

            scrollRegions.AddRegion(scr.segmentedAddress, scr.vertexCount * 0x10);
        }

        public SortedRegionList vertexRegions;
        public void RegisterVertex(int segAddr)
        {
            vertexRegions.AddRegion(segAddr, 0x10);
        }
    }

    public class VerticesDescription
    {
        // 04 cmd -> set of BF cmd
        SortedDictionary<UInt64, SortedSet<UInt64>> data;

        public VerticesDescription()
        {
            data = new SortedDictionary<ulong, SortedSet<ulong>>();
        }

        public void Add(UInt64 vertexCmd, UInt64 triCmd)
        {
            if (!data.TryGetValue(vertexCmd, out SortedSet<ulong> set))
            {
                set = new SortedSet<UInt64>();
                data[vertexCmd] = set;
            }

            set.Add(triCmd);
        }

        public void MakeF3D(ROM rom)
        {
            foreach (UInt64 cmd04 in data.Keys)
            {
                rom.Write64(cmd04);
                rom.AddOffset(8);

                SortedSet<UInt64> set = data[cmd04];
                foreach (UInt64 cmdBF in set)
                {
                    rom.Write64(cmdBF);
                    rom.AddOffset(8);
                }
            }
        }
    }

    public class VertexBufferDescription
    {
        int size;
        public Vertex[] vbuf;
        public int freeCount { get; private set; }
        public int usedCount { get => size - freeCount; private set => freeCount = size - value; }

        readonly UInt32 segmentedAddress;

        public List<UInt64> triCmds;

        public VertexBufferDescription(UInt32 segmentedAddress, int size)
        {
            this.size = size;
            this.segmentedAddress = segmentedAddress;
            vbuf = new Vertex[size];
            freeCount = size;
            triCmds = new List<UInt64>();
        }

        public int FindVertex(Vertex v)
        {
            return Array.FindIndex(vbuf, vb => v.Equals(vb));
        }

        public int AddVertex(Vertex v)
        {
            return AddVertex(v, out bool b);
        }

        public int AddVertex(Vertex v, out bool isNew)
        {
            int index = FindVertex(v);
            if (index != -1)
            {
                isNew = false;
                return index;
            }

            if (freeCount == 0)
                throw new OutOfMemoryException("No more space in Vertex Buffer");

            index = usedCount;
            vbuf[index] = v;
            freeCount--;
            isNew = true;
            return index;
        }

        public void DrawTriangle(int[] indices)
        {
            if (indices.Length != 3)
                throw new ArgumentException("3 Indices must be provided!");
            
            foreach (int i in indices)
            {
                if (i >= usedCount)
                    throw new ArgumentException("Invalid index provided");
            }

            PositionalBuffer triCmd = new PositionalBuffer(new byte[8]);
            triCmd.Write64(0xBF00000000000000);
            for (int i = 0; i < 3; i++)
            {
                int index = indices[i];
                triCmd.Write8((byte)(index * 0xA), 5 + i);
            }
            triCmds.Add((UInt64)triCmd.Read64());
        }

        public void DrawTriangle(Triangle tri)
        {
            int[] indices = new int[3];

            for (int i = 0; i < 3; i++)
            {
                Vertex v = tri.vertices[i];
                indices[i] = AddVertex(v);
            }

            DrawTriangle(indices);
        }

        public void MakeData(ROM rom)
        {
            for (int i = 0; i < usedCount; i++)
            {
                Vertex v = vbuf[i];
                v.MakeData(rom);
            }
        }

        public void MakeF3D(ROM rom)
        {
            PositionalBuffer vertexCmdBuf = new PositionalBuffer(new byte[8]);
            vertexCmdBuf.Write64(0x0400000000000000);
            vertexCmdBuf.Write8((byte)((usedCount - 1) << 4), 1);
            vertexCmdBuf.Write16((short)(usedCount * 0x10), 2);
            vertexCmdBuf.Write32((Int32)segmentedAddress, 4);
            UInt64 vertexCmd = (UInt64)vertexCmdBuf.Read64();

            VerticesDescription vd = new VerticesDescription();
            foreach (UInt64 triCmd in triCmds)
            {
                vd.Add(vertexCmd, triCmd);
            }
            vd.MakeF3D(rom);
        }
    }

    public class Vertex : IEquatable<Vertex>
    {
        public readonly UInt64 lo;
        public readonly UInt64 hi;

        public Vertex(UInt64 lo, UInt64 hi)
        {
            this.lo = lo;
            this.hi = hi;
        }

        public void MakeData(ROM rom)
        {
            rom.Write64(lo);
            rom.Write64(hi, 8);
            rom.AddOffset(16);
        }

        public bool Equals(Vertex other)
        {
            if (other == null)
                return false;

            return lo == other.lo && hi == other.hi;
        }

        public override int GetHashCode()
        {
            return lo.GetHashCode() ^ hi.GetHashCode();
        }

        public override string ToString()
        {
            return (GetHashCode() & 0xFFFFFF).ToString();
        }
    }

    public class Triangle
    {
        public readonly Vertex[] vertices;

        public Triangle(Vertex v0, Vertex v1, Vertex v2)
        {
            if (v0 == null || v1 == null || v2 == null)
                throw new ArgumentException("Vertex can't be null");

            vertices = new Vertex[3];
            vertices[0] = v0;
            vertices[1] = v1;
            vertices[2] = v2;
        }

        public int CountCommon(Triangle other)
        {
            int count = 0;
            foreach (Vertex v in vertices)
                foreach (Vertex otherv in other.vertices)
                    if (v.Equals(otherv))
                        count++;

            return count;
        }

        public bool Contains(Vertex other)
        {
            foreach (Vertex v in vertices)
            {
                if (v.Equals(other))
                    return true;
            }

            return false;
        }

        public override string ToString()
        {
            return String.Format("{0} {1} {2}", vertices[0], vertices[1], vertices[2]);
        }
    }

    public class VisualMap
    {
        List<UInt64> header;
        Dictionary<TextureDescription, VerticesDescription> map;
        List<UInt64> footer;

        public VisualMap()
        {
            map = new Dictionary<TextureDescription, VerticesDescription>();
            header = new List<UInt64>();
            footer = new List<UInt64>();
        }

        public void AddHeaderCmd(UInt64 cmd)
        {
            header.Add(cmd);
        }

        public void AddFooterCmd(UInt64 cmd)
        {
            footer.Add(cmd);
        }

        public void AddTriangle(TextureDescription td, UInt64 vertexCmd, UInt64 triCmd)
        {
            VerticesDescription set = null;
            if (!map.TryGetValue(td, out set))
            {
                set = new VerticesDescription();
                map[td] = set;
            }

            set.Add(vertexCmd, triCmd);
        }

        public int MakeF3D(ROM rom)
        {
            int start = rom.offset;
            foreach(UInt64 cmd in header)
            {
                rom.Write64(cmd);
                rom.AddOffset(8);
            }

            List<TextureDescription> tds = map.Keys.ToList();
            tds.Sort(new TextureDescriptionComp());

            foreach (TextureDescription td in tds)
            {
                foreach (UInt64 cmd in (List<UInt64>)td)
                {
                    rom.Write64(cmd);
                    rom.AddOffset(8);
                }

                VerticesDescription vd = map[td];
                vd.MakeF3D(rom);
            }
            foreach (UInt64 cmd in footer)
            {
                rom.Write64(cmd);
                rom.AddOffset(8);
            }
            return (rom.offset - start);
        }
    }

    public class TextureDescriptionComp : IComparer<TextureDescription>
    {
        // Compares by Height, Length, and Width.
        public int Compare(TextureDescription x, TextureDescription y)
        {
            return x.GetTextureCMD().CompareTo(y.GetTextureCMD());
        }
    }

    public class ScrollingTextureDescriptionComp : IComparer<ScrollingTextureDescription>
    {
        // Compares by Height, Length, and Width.
        public int Compare(ScrollingTextureDescription x, ScrollingTextureDescription y)
        {
            int cmp = x.GetTextureCMD().CompareTo(y.GetTextureCMD());
            if (cmp != 0)
                return cmp;

            if (x.scroll == null && y.scroll == null)
                return 0;

            if (x.scroll == null && y.scroll != null)
                return 1;

            if (x.scroll != null && y.scroll == null)
                return -1;

            return 0; //x.scroll.CompareTo(y.scroll);
        }
    }

    public class ScrollFactory
    {
        public ScrollFactory(List<ScrollObject> scrollers)
        {
            editorScrollBehaviour = 0x402300;
            this.scrollers = scrollers;
        }

        public List<EditorScroll> GetScrolls(int vertexCount, int segmentedAddress, byte speed, byte acts, TextureAxis axis)
        {
            List<EditorScroll> scrolls = new List<EditorScroll>();
            /*
            while (vertexCount != 0)
            {
                ScrollObject scr = Fetch();
                if (vertexCount > 0xff * 3)
                {
                    vertexCount -= 0xff * 3;
                    scrolls.Add(new EditorScroll(0xff * 3, segmentedAddress, speed, acts, axis, editorScrollBehaviour, scr.romOffset));
                }
                else
                {
                    scrolls.Add(new EditorScroll(vertexCount, segmentedAddress, speed, acts, axis, editorScrollBehaviour, scr.romOffset));
                    vertexCount = 0;
                }
            }
            */
            ScrollObject scr = Fetch();
            scrolls.Add(new EditorScroll(vertexCount, segmentedAddress, speed, acts, axis, editorScrollBehaviour, scr.romOffset));
            return scrolls;
        }

        ScrollObject Fetch()
        {
            ScrollObject scr = scrollers[0];
            scrollers.Remove(scr);
            return scr;
        }

        int editorScrollBehaviour;
        List<ScrollObject> scrollers;
    }

    // TODO: The way scrolls are implemented is bad: it will not change amount of scrolls used even though it should merge existing ones :(
    // This will lead to bad separations of data
    public class TriangleMap
    {
        const int vertexPresentBonus = 100000; // should be good enough

        List<UInt64> header;
        List<UInt64> footer;
        public Dictionary<ScrollingTextureDescription, List<Triangle>> map;

        public TriangleMap()
        {
            map = new Dictionary<ScrollingTextureDescription, List<Triangle>>();
            header = new List<UInt64>();
            footer = new List<UInt64>();
        }

        public void AddHeaderCmd(UInt64 cmd)
        {
            header.Add(cmd);
        }

        public void AddFooterCmd(UInt64 cmd)
        {
            footer.Add(cmd);
        }

        public void AddTriangle(ScrollingTextureDescription td, Vertex v0, Vertex v1, Vertex v2)
        {
            if (!map.TryGetValue(td, out List<Triangle> set))
            {
                set = new List<Triangle>();
                map[td] = set;
            }
            Triangle tri = new Triangle(v0, v1, v2);

            set.Add(tri);
        }

        public int MakeF3D(ROM rom, SortedRegionList vertexBytes, ScrollFactory factory)
        {
            int start = rom.offset;
            foreach (UInt64 cmd in header)
            {
                rom.Write64(cmd);
                rom.AddOffset(8);
            }

            // TODO: This is kinda stupid assumption but we believe there are more triangles in the beginning available...
            List<ScrollingTextureDescription> tds = map.Keys.ToList();
            tds.Sort(new ScrollingTextureDescriptionComp());

            TextureDescription prevTd = null;
            foreach (ScrollingTextureDescription td in tds)
            {
                if (prevTd != td)
                    foreach (UInt64 cmd in (List<UInt64>)td)
                    {
                        rom.Write64(cmd);
                        rom.AddOffset(8);
                    }

                prevTd = td;

                // Time to do optimization magic
                List<Triangle> textureTris = map[td];
                
                // Create reverse map for vertices, vertices with more triangles will be used first
                Dictionary<Vertex, List<Triangle>> vertex2triMap = new Dictionary<Vertex, List<Triangle>>();
                foreach (Triangle tri in textureTris)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vertex v = tri.vertices[i];
                        if (!vertex2triMap.TryGetValue(v, out List<Triangle> weight))
                        {
                            vertex2triMap[v] = new List<Triangle>();
                        }
                        vertex2triMap[v].Add(tri);
                    }
                }

                // You quite literally can't eat more than that
                int vertexLength = 0x30 * textureTris.Count();
                vertexBytes.CutContigRegion(vertexLength, out int vertexPosition);

                int allocVertexStart = vertexPosition;
                int allocVertexEnd = allocVertexStart + vertexLength;
                int writtenVerticesCount = 0;

                // Check if there are still vertices that needs to be worked with
                // Also we must make sure all lists in v2tm are not empty!
                while (vertex2triMap.Count != 0)
                {
                    int segmentedVertexStart = rom.GetSegmentedAddress(vertexPosition);

                    // vbd is main structure that holds information about used vertices space
                    VertexBufferDescription vbd = new VertexBufferDescription((UInt32) segmentedVertexStart, 0xF);

                    // Potential triangles also have weight depending on connection to other triangles and to added vertices
                    // Weight +1 for each tri that shares vertice; +vertexPresentBonus for each vbd present vertex
                    // It is recommended to have vertexPresentBonus be more than any weight that shared vertices can give
                    // This way triangle that have the most weight will have the most vertices and potentially help others
                    Dictionary<Triangle, int> potentialTris = new Dictionary<Triangle, int>();

                    // Add triangles to vbd till it won't be able to fit in the whole triangle
                    while (vbd.freeCount != 0 && vertex2triMap.Count != 0)
                    {
                        // Right now no vertices are in vbd, pick one with the heighest weight and add it to vbd
                        if (potentialTris.Count == 0)
                        {
                            // We need at least 3 vertices to draw not present tri
                            if (vbd.freeCount < 3)
                                break;

                            KeyValuePair<Vertex, List<Triangle>> pair;
                            // Let's ask for at least 5 vertices for optimal stuff, otherwise just put the worst triangle and call it
                            if (vbd.freeCount < 5)
                            {
                                pair = vertex2triMap.Aggregate((prev, cur) => prev.Value.Count < cur.Value.Count ? prev : cur);
                            }
                            else
                            {
                                pair = vertex2triMap.Aggregate((prev, cur) => prev.Value.Count > cur.Value.Count ? prev : cur);
                            }

                            Vertex v = pair.Key;
                            List<Triangle> tris = pair.Value;
                            if (tris.Count == 0)
                                throw new ArithmeticException("Tris count in dict can't be 0");

                            // Put current vertex and use found tris as potential next tri to add
                            vbd.AddVertex(v);

                            // Setup all potential triangles to could be added to vbd, the ones that have this vertex in common

                            foreach (Triangle tri in tris)
                            {
                                potentialTris[tri] = 0;
                            }

                            // If vertex is in common between tris, give 1 each
                            var tripairs = tris.Zip(tris.Skip(1), (a, b) => Tuple.Create(a, b));
                            foreach (Tuple<Triangle, Triangle> tripair in tripairs)
                            {
                                Triangle tri1 = tripair.Item1;
                                Triangle tri2 = tripair.Item2;
                                int common = tri1.CountCommon(tri2);

                                potentialTris[tri1] += common;
                                potentialTris[tri2] += common;
                            }
                        }
                        

                        // Find tri with biggest weight
                        Triangle biggestTri = potentialTris.Aggregate((prev, cur) => prev.Value > cur.Value ? prev : cur).Key;

                        // Add all triangles in vbd
                        int[] indices = new int[3];

                        for (int i = 0; i < 3; i++)
                        {
                            Vertex v = biggestTri.vertices[i];
                            int index = vbd.AddVertex(v, out bool isNew);
                            indices[i] = index;

                            // Fix weights of tris if a new vertex was added to vbd (not already in vbd it is)
                            if (isNew)
                                foreach (Triangle ptri in potentialTris.Keys.ToList())
                                    if (ptri.Contains(v))
                                        potentialTris[ptri] += vertexPresentBonus;

                        }

                        // Draw the triangle, all vertices are guaranteed to be in
                        vbd.DrawTriangle(indices);

                        // As triangle was drawn, it needs to be removed from structs that had it
                        // vertex2map & potentialTris
                        // This way triangle won't be able to appear again
                        foreach (Vertex v in biggestTri.vertices)
                        {
                            vertex2triMap[v].Remove(biggestTri);
                            if (vertex2triMap[v].Count == 0)
                                vertex2triMap.Remove(v);
                        }

                        potentialTris.Remove(biggestTri);

                        // Setup new triangles to potentialTris
                        HashSet<Triangle> newTris = new HashSet<Triangle>();
                        foreach (Vertex v in biggestTri.vertices)
                        {
                            if (vertex2triMap.TryGetValue(v, out List<Triangle> tris))
                                foreach (Triangle tri in tris)
                                    newTris.Add(tri);
                        }

                        // Also do not take into account tris that were added already
                        // If we will take them into account, bad things will happen
                        newTris.RemoveWhere(t => potentialTris.Keys.Contains(t));

                        // As new triangles appear, proceed to fix weights
                        // First of all, initialize world: create new potential tris that will be merged later with potential tris
                        // Initial value is -vertexPresentBonus as one vertex will be counted anyways
                        Dictionary<Triangle, int> newPotentialTris = new Dictionary<Triangle, int>();
                        foreach (Triangle ntri in newTris)
                            newPotentialTris[ntri] = -vertexPresentBonus;

                        // Calculate weights for vertices that were present
                        foreach (Triangle ntri in newTris)
                            foreach (Vertex v in vbd.vbuf)
                                if (ntri.Contains(v))
                                    newPotentialTris[ntri] += vertexPresentBonus;

                        // Check between triangles new/new
                        var newNewTripairs = newTris.Zip(newTris.Skip(1), (a, b) => Tuple.Create(a, b));
                        foreach (Tuple<Triangle, Triangle> tripair in newNewTripairs)
                        {
                            Triangle tri1 = tripair.Item1;
                            Triangle tri2 = tripair.Item2;
                            int common = tri1.CountCommon(tri2);

                            newPotentialTris[tri1] += common;
                            newPotentialTris[tri2] += common;
                        }

                        // Check between triangles new/old
                        List<Triangle> oldTris = potentialTris.Keys.ToList();
                        var newOldTripairs = newTris.Zip(oldTris, (a, b) => Tuple.Create(a, b));
                        foreach (Tuple<Triangle, Triangle> tripair in newOldTripairs)
                        {
                            Triangle newTri = tripair.Item1;
                            Triangle oldTri = tripair.Item2;

                            int common = newTri.CountCommon(oldTri);

                            newPotentialTris[newTri] += common;
                            potentialTris[oldTri]    += common;
                        }
                    
                        // Merge together old and new
                        foreach (KeyValuePair<Triangle, int> pair in newPotentialTris)
                            potentialTris.Add(pair.Key, pair.Value);

                        // Check if there are triangles that have all vertexes inside the vbd
                        // All such vertices could be initially drawn so just draw them
                        // Triangles with weight over 2*vertexPresetBonus have 2+1 vertices in buffer so just draw them
                        var drawnPairs = potentialTris.Where((t, w) => w >= 2 * vertexPresentBonus);
                        foreach (KeyValuePair<Triangle, int> pair in drawnPairs)
                        {
                            Triangle ftri = pair.Key;
                            for (int i = 0; i < 3; i++)
                            {
                                Vertex v = ftri.vertices[i];
                                int index = vbd.AddVertex(v, out bool isNew);
                                indices[i] = index;

                                // Fix weights of tris if a new vertex was added to vbd (not already in vbd it is)
                                if (isNew)
                                    throw new ArithmeticException("Triangles with high weight has new vertices!");
                            }

                            // Draw the triangle, all vertices are guaranteed to be in
                            vbd.DrawTriangle(indices);
                            potentialTris.Remove(biggestTri);
                        }

                        // Check if there are less then 2 vertices, which are required to be able to draw any tri
                        // If there is a vertex that has vertexPresentBonus weight, we good, otherwise retreat
                        if (vbd.freeCount == 1)
                            if (potentialTris.Values.ToList().FindIndex(w => w >= vertexPresentBonus) == -1)
                                break;
                    }

                    rom.PushOffset(vertexPosition);
                    vbd.MakeData(rom);
                    writtenVerticesCount += (rom.offset - vertexPosition) / 0x10;
                    vertexPosition = rom.offset;
                    rom.PopOffset();

                    vbd.MakeF3D(rom);
                }

                if (td.scroll != null)
                {
                    // Vertices must be rounded by 3 because skelux scrolls work like that
                    if (writtenVerticesCount % 3 != 0)
                    {
                        int leftToRoundVertices = 3 - (writtenVerticesCount % 3);
                        writtenVerticesCount += leftToRoundVertices;
                    }

                    int segmentedAddress = rom.GetSegmentedAddress(allocVertexStart);
                    List<EditorScroll> scrolls = factory.GetScrolls(writtenVerticesCount, segmentedAddress, td.scroll.speed, td.scroll.acts, td.scroll.axis);
                    foreach(EditorScroll scroll in scrolls)
                    {
                        scroll.WriteScroll(rom);
                    }
                }

                vertexBytes.AddRegion(vertexPosition, allocVertexEnd - vertexPosition);
            }
            foreach (UInt64 cmd in footer)
            {
                rom.Write64(cmd);
                rom.AddOffset(8);
            }
            return (rom.offset - start);
        }
    }
}
