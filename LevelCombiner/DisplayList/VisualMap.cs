using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LevelCombiner
{
    // List of F3D commands basically
    public class TextureDescription : List<UInt64>
    {
        public override bool Equals(object obj)
        {
            List<UInt64> list = (List<UInt64>)(obj);
            return Enumerable.SequenceEqual(this, list);
        }

        public override int GetHashCode()
        {
            int hash = 0;
            foreach (UInt64 cmd in this)
            {
                hash ^= cmd.GetHashCode();
            }
            return hash;
        }

        public UInt64 GetTextureCMD()
        {
            foreach (UInt64 cmd in this)
            {
                if ((cmd & 0xFF00000000000000) == 0xFD00000000000000)
                    return cmd;
            }
            return 0;
        }

        public override string ToString()
        {
            foreach (UInt64 cmd in this)
            {
                if ((cmd & 0xFF000000) == 0xFD000000)
                    return cmd.ToString();
            }

            return base.ToString();
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
                foreach (UInt64 cmd in td)
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

    public class TriangleMap
    {
        const int vertexPresentBonus = 100000; // should be good enough

        List<UInt64> header;
        Dictionary<TextureDescription, List<Triangle>> map;
        SortedRegionList vertexBytes;
        List<UInt64> footer;

        public TriangleMap()
        {
            map = new Dictionary<TextureDescription, List<Triangle>>();
            vertexBytes = new SortedRegionList();
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

        public void AddVertexRegion(Region region)
        {
            vertexBytes.AddRegion(region.romStart, region.length);
        }

        public void AddTriangle(TextureDescription td, Vertex v0, Vertex v1, Vertex v2)
        {
            if (!map.TryGetValue(td, out List<Triangle> set))
            {
                set = new List<Triangle>();
                map[td] = set;
            }
            Triangle tri = new Triangle(v0, v1, v2);

            set.Add(tri);
        }

        public int MakeF3D(ROM rom)
        {
            int start = rom.offset;
            foreach (UInt64 cmd in header)
            {
                rom.Write64(cmd);
                rom.AddOffset(8);
            }

            List<TextureDescription> tds = map.Keys.ToList();
            tds.Sort(new TextureDescriptionComp());

            foreach (TextureDescription td in tds)
            {
                foreach (UInt64 cmd in td)
                {
                    rom.Write64(cmd);
                    rom.AddOffset(8);
                }

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
                        else
                        {
                            Console.Write("ok");
                        }
                        vertex2triMap[v].Add(tri);
                    }
                }

                // Check if there are still vertices that needs to be worked with
                // Also we must make sure all lists in v2tm are not empty!
                while (vertex2triMap.Count != 0)
                {
                    // Cut a bit from available space for vertices
                    KeyValuePair<int, int> region = vertexBytes.RegionList.First();
                    vertexBytes.RegionList.RemoveAt(0);
                    int vertexStart = region.Key;
                    int vertexLength = region.Value;

                    bool isRegionTrimmed = vertexLength > 0x100;
                    // vertex buffer is 0x100 max size
                    if (isRegionTrimmed)
                    {
                        // Trim and put back data if left
                        vertexBytes.RegionList.Add(vertexStart + 0x100, vertexLength - 0x100);
                        vertexLength = 0x100;
                    }
                    else
                    {
                        // Round to closest 0x10
                        vertexLength = vertexLength / 0x10 * 0x10;
                    }

                    int segmentedVertexStart = rom.GetSegmentedAddress(vertexStart);

                    // vbd is main structure that holds information about used vertices space
                    VertexBufferDescription vbd = new VertexBufferDescription((UInt32) segmentedVertexStart, vertexLength / 0x10);

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

                    // If region was trimmed (aka it was not small), bring back unused parts
                    if (isRegionTrimmed && vbd.freeCount != 0)
                    {
                        int size = vbd.freeCount * 0x10;
                        int offset = vbd.usedCount * 0x10;
                        vertexBytes.AddRegion(vertexStart + offset, size);
                    }

                    rom.PushOffset(vertexStart);
                    vbd.MakeData(rom);
                    rom.PopOffset();

                    vbd.MakeF3D(rom);
                }
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
