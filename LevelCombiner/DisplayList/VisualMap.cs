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

    public class Vertex : IEquatable<Vertex>
    {
        public readonly UInt64 lo;
        public readonly UInt64 hi;

        public Vertex(UInt64 lo, UInt64 hi)
        {
            this.lo = lo;
            this.hi = hi;
        }

        public bool Equals(Vertex other)
        {
            return lo == other.lo && hi == other.hi;
        }

        public override int GetHashCode()
        {
            return lo.GetHashCode() ^ hi.GetHashCode();
        }
    }

    public class Triangle
    {
        public readonly Vertex[] vertices;

        public Triangle(Vertex v0, Vertex v1, Vertex v2)
        {
            vertices = new Vertex[3];
            vertices[0] = v0;
            vertices[1] = v1;
            vertices[2] = v2;
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
            if (!map.TryGetValue(td, out VerticesDescription set))
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
        List<UInt64> header;
        Dictionary<TextureDescription, List<Triangle>> map;
        MergedRegionList vertexBytes;
        List<UInt64> footer;

        public TriangleMap()
        {
            map = new Dictionary<TextureDescription, List<Triangle>>();
            vertexBytes = new MergedRegionList();
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

        public void AddTriangle(TextureDescription td, UInt64 vertexCmd, UInt64 triCmd, ROM rom)
        {
            if (!map.TryGetValue(td, out List<Triangle> set))
            {
                set = new List<Triangle>();
                map[td] = set;
            }

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

                //VerticesDescription vd = map[td];
                //vd.MakeF3D(rom);
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
