using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LevelCombiner
{
    class GeoLayout
    {
        const int size = 0x21;
        delegate void RegionParseCmd(ROM rom, List<Region> regions);
        delegate void RelocationParseCmd(ROM rom, RelocationTable table);
        static RegionParseCmd[] parser = new RegionParseCmd[size];
        static RelocationParseCmd[] relocationParser = new RelocationParseCmd[size];

                                            /*   0    1     2     3      4     5     6     7*/
                                            /*   8    9     A     B      C     D     E     F*/
        static int[] cmdSizeTable = new int[] { 0x08, 0x04, 0x08, 0x04, 0x04, 0x04, 0x00, 0x00,
                                                0x0C, 0x04, 0xFF, 0x04, 0x04, 0x08, 0x08, 0x14,
                                                0x10, 0x00, 0x00, 0x0C, 0x08, 0x08, 0x08, 0x04,
                                                0x08, 0x08, 0x08, 0x00, 0x00, 0xFF, 0x08, 0x10,
                                                0x04};
        static GeoLayout()
        {
            Type t = typeof(GeoLayout);
            for (int i = 0; i < size; i++)
            {
                parser[i] = RegionParse_common;

                string name = "RegionParse_cmd" + string.Format("{0:X2}", i);
                MethodInfo info = t.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
                if (info == null)
                    continue;

                RegionParseCmd cmd = Delegate.CreateDelegate(typeof(RegionParseCmd), info) as RegionParseCmd;
                if (cmd == null)
                    continue;

                parser[i] = cmd;
            }

            for (int i = 0; i < size; i++)
            {
                relocationParser[i] = RelocationParse_common;

                string name = "RelocationParse_cmd" + string.Format("{0:X2}", i);
                MethodInfo info = t.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
                if (info == null)
                    continue;

                RelocationParseCmd cmd = Delegate.CreateDelegate(typeof(RelocationParseCmd), info) as RelocationParseCmd;
                if (cmd == null)
                    continue;

                relocationParser[i] = cmd;
            }
        }

        public static void PerformRegionParse(ROM rom, List<Region> regions, int offset)
        {
            rom.PushOffset(offset);
            try
            {
                List<Region> displayListRegions = new List<Region>();

                int cmd;
                do
                {
                    cmd = rom.Read8();
                    int cmdSize = cmdSizeTable[cmd];
                    if (cmdSize == 0)
                        throw new ArgumentException("Loop detected");

                    parser[cmd](rom, displayListRegions);

                    if (cmdSize != 0xFF)
                        rom.AddOffset(cmdSize);
                } while (cmd != 0x01);

                // !!! : Group together all display list information in geolayout
                MergedRegionList compoundGraphicsData = new MergedRegionList();
                List<Region> otherRegions = new List<Region>();

                int count = 0;
                // Merge together vertex, texture, light descriptions
                // Enumerate display lists
                foreach (Region reg in displayListRegions)
                {
                    switch (reg.state)
                    {
                        case RegionState.GraphicsData:
                            compoundGraphicsData.AddRegion(reg.romStart, reg.length);
                            break;
                        case RegionState.DisplayList:
                            reg.number = count++;
                            otherRegions.Add(reg);
                            break;
                        default:
                            // Passthrough region that we do not need to compound
                            otherRegions.Add(reg);
                            break;
                    }
                }
                
                Region graphicsRegion = new DynamicRegion(compoundGraphicsData.start, compoundGraphicsData.length, RegionState.GraphicsData);
                regions.Add(graphicsRegion);

                regions.AddRange(otherRegions);

                Region region = new GeoLayoutRegion(offset, rom.offset - offset);
                regions.Add(region);
            }
            finally
            {
                rom.PopOffset();
            }
        }

        public static void PerformRegionRelocation(Region region, RelocationTable table)
        {
            // This is fake rom but it works anyways, just more convenient
            ROM rom = new ROM(region.data);

            byte cmd;
            do
            {
                cmd = rom.Read8();
                int cmdSize = cmdSizeTable[cmd];
                if (cmdSize == 0)
                    throw new ArgumentException("Loop detected");

                RelocationParseCmd func = relocationParser[cmd];
                func(rom, table);

                if (cmdSize != 0xFF)
                    rom.AddOffset(cmdSize);
            }
            while (rom.offset < region.length);
        }

        private static void RegionParse_common(ROM rom, List<Region> regions) { }

        private static void RelocationParse_common(ROM rom, RelocationTable table) { }
        
        private static void RegionParse_cmd0A(ROM rom, List<Region> regions)
        {
            int useAsm = rom.Read8(1);
            rom.AddOffset(useAsm == 0 ? 0x8 : 0xC);
        }

        private static void RelocationParse_cmd0A(ROM rom, RelocationTable table)
        {
            int useAsm = rom.Read8(1);
            rom.AddOffset(useAsm == 0 ? 0x8 : 0xC);
        }

        private static void RegionParse_cmd13(ROM rom, List<Region> regions)
        {
            int segment = rom.Read8(8);
            if (segment != 0x0e)
                return;

            int segmentedAddress = rom.Read32(8);
            int address = (int)rom.GetROMAddress(segmentedAddress);

            DisplayList.PerformRegionParse(rom, regions, address, rom.Read8(1));
        }

        private static void RelocationParse_cmd13(ROM rom, RelocationTable table)
        {
            table.RelocateOffset(rom, 8);
        }

        private static void RegionParse_cmd15(ROM rom, List<Region> regions)
        {
            int segment = rom.Read8(4);
            if (segment != 0x0e)
                return;

            int segmentedAddress = rom.Read32(4);
            int address = (int)rom.GetROMAddress(segmentedAddress);

            DisplayList.PerformRegionParse(rom, regions, address, rom.Read8(1));
        }

        private static void RelocationParse_cmd15(ROM rom, RelocationTable table)
        {
            table.RelocateOffset(rom, 4);
        }
        
        private static void RegionParse_cmd1D(ROM rom, List<Region> regions)
        {
            // TODO: msbit thing
            rom.AddOffset(0x8);
        }

        private static void RelocationParse_cmd1D(ROM rom, RelocationTable table)
        {
            // TODO: msbit thing
            rom.AddOffset(0x8);
        }
    }
}
