using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LevelCombiner
{
    class Collision
    {
        static RegionParseCmd[] parser = new RegionParseCmd[5];

        static Collision()
        {
            Type t = typeof(Collision);
            for (int i = 0x40; i < 0x45; i++)
            {
                parser[i - 0x40] = RegionParse_common;

                string name = "RegionParse_cmd" + string.Format("{0:X2}", i);
                MethodInfo info = t.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
                if (info == null)
                    continue;

                RegionParseCmd cmd = Delegate.CreateDelegate(typeof(RegionParseCmd), info) as RegionParseCmd;
                if (cmd == null)
                    continue;

                parser[i - 0x40] = cmd;
            }
        }

        public delegate void RegionParseCmd(ROM rom, List<Region> regions);

        public static void PerformRegionParse(ROM rom, List<Region> regions, int offset)
        {
            rom.PushOffset(offset);
            try
            {
                int cmd = 0;
                do
                {
                    if (rom.Read8() != 0x00)
                        throw new ArgumentException("invalid instruction");

                    cmd = rom.Read8(1);
                    if (cmd >= 0x40 && cmd <= 0x44)
                        parser[cmd - 0x40](rom, regions);
                    else
                        RegionParse_common(rom, regions);
                }
                while (cmd != 0x42);

                Region region = new DynamicRegion(offset, rom.offset - offset, RegionState.Collision);
                regions.Add(region);
            }
            finally
            {
                rom.PopOffset();
            }
        }

        private static void RegionParse_common(ROM rom, List<Region> regions)
        {
            // Triangles of any collision

            int size = rom.Read16(2);
            if (size == 0)
                throw new ArgumentException("common collision loop detected");

            int collType = rom.Read8(1);
            int triangleSize = 6;

            switch(collType)
            {
                case 0x0E:
                case 0x24:
                case 0x25:
                case 0x27:
                case 0x2C:
                case 0x2D:
                    triangleSize = 8;
                    break;
            }

            rom.AddOffset(4 + size * triangleSize);
        }

        private static void RegionParse_cmd40(ROM rom, List<Region> regions)
        {
            // Vertexes
            int size = rom.Read16(2);
            rom.AddOffset(4 + size * 6);
        }

        private static void RegionParse_cmd41(ROM rom, List<Region> regions)
        {
            rom.AddOffset(2);
        }

        private static void RegionParse_cmd42(ROM rom, List<Region> regions)
        {
            rom.AddOffset(2);
        }

        private static void RegionParse_cmd43(ROM rom, List<Region> regions)
        {
            throw new ArgumentException("collision: unknown 0x43");
        }

        private static void RegionParse_cmd44(ROM rom, List<Region> regions)
        {
            // Wata
            // Vertexes
            int size = rom.Read16(2);
            rom.AddOffset(4 + size * 12);
        }
    }
}
