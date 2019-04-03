using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LevelCombiner
{
    class DisplayList
    {
        delegate void RegionParseCmd(ROM rom, List<Region> regions, RegionParseState state);
        delegate void FixParseCmd(ROM rom, DisplayListRegion region, RegionFixState state);
        delegate void OptimizeParserCmd(ROM rom, DisplayListRegion region, RegionOptimizeState state);
        delegate void VisualMapParserCmd(ROM rom, VisualMap map, VisualMapParseState state);
        delegate void TriangleMapParserCmd(ROM rom, TriangleMap map, TriangleMapParseState state);

        static RegionParseCmd[] parser = new RegionParseCmd[0xFF];
        static FixParseCmd[] fixParser = new FixParseCmd[0xFF];
        static OptimizeParserCmd[] optimizeParser = new OptimizeParserCmd[0xFF];
        static VisualMapParserCmd[] visualMapParser = new VisualMapParserCmd[0xFF];
        static TriangleMapParserCmd[] triangleMapParser = new TriangleMapParserCmd[0xFF];

        static DisplayList()
        {
            Type t = typeof(DisplayList);
            for (int i = 0x00; i < 0xFF; i++)
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

            for (int i = 0; i < 0xFF; i++)
            {
                fixParser[i] = FixParse_common;

                string name = "FixParse_cmd" + string.Format("{0:X2}", i);
                MethodInfo info = t.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
                if (info == null)
                    continue;

                FixParseCmd cmd = Delegate.CreateDelegate(typeof(FixParseCmd), info) as FixParseCmd;
                if (cmd == null)
                    continue;

                fixParser[i] = cmd;
            }

            for (int i = 0; i < 0xFF; i++)
            {
                optimizeParser[i] = OptimizeParse_common;

                string name = "OptimizeParse_cmd" + string.Format("{0:X2}", i);
                MethodInfo info = t.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
                if (info == null)
                    continue;

                OptimizeParserCmd cmd = Delegate.CreateDelegate(typeof(OptimizeParserCmd), info) as OptimizeParserCmd;
                if (cmd == null)
                    continue;

                optimizeParser[i] = cmd;
            }

            for (int i = 0; i < 0xFF; i++)
            {
                visualMapParser[i] = VisualMapParse_common;

                string name = "VisualMapParse_cmd" + string.Format("{0:X2}", i);
                MethodInfo info = t.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
                if (info == null)
                    continue;

                VisualMapParserCmd cmd = Delegate.CreateDelegate(typeof(VisualMapParserCmd), info) as VisualMapParserCmd;
                if (cmd == null)
                    continue;

                visualMapParser[i] = cmd;
            }

            for (int i = 0; i < 0xFF; i++)
            {
                triangleMapParser[i] = TriangleMapParse_common;

                string name = "TriangleMapParse_cmd" + string.Format("{0:X2}", i);
                MethodInfo info = t.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
                if (info == null)
                    continue;

                TriangleMapParserCmd cmd = Delegate.CreateDelegate(typeof(TriangleMapParserCmd), info) as TriangleMapParserCmd;
                if (cmd == null)
                    continue;

                triangleMapParser[i] = cmd;
            }
        }

        public struct FixConfig
        {
            public bool nerfFog;
            public bool optimize;
            public bool trimNOPs;
            public bool groupByTextures;
            public bool fixCombiners;
            public bool fixOtherMode;
            public bool disableFog;

            public FixConfig(bool nerfFog, bool optimizeVertex, bool trimNOPs, bool groupByTextures, bool fixCombiners, bool fixOtherMode, bool disableFog)
            {
                this.nerfFog = nerfFog;
                this.optimize = optimizeVertex;
                this.trimNOPs = trimNOPs;
                this.groupByTextures = groupByTextures;
                this.fixCombiners = fixCombiners;
                this.fixOtherMode = fixOtherMode;
                this.disableFog = disableFog;
            }
        }

        class RegionParseState
        {
            public SortedRegionList textureData;
            public SortedRegionList vertexData;
            public SortedRegionList lightData;
            public Stack<int> retAddressStack;
            public int FDAddress;

            public bool isFogEnabled;
            public bool isEnvColorEnabled;
            public int FCCount;
            public Int64 FCcmd;
            public Int64 B9cmdfirst;
            public bool isUnusedTrimmingAllowed;

            public RegionParseState()
            {
                textureData = new SortedRegionList();
                vertexData = new SortedRegionList();
                lightData = new SortedRegionList();
                FDAddress = -1;
                FCcmd = -1;
                isUnusedTrimmingAllowed = true;

                retAddressStack = new Stack<int>();
            }
        }

        class RegionFixState
        {
            public int FCCountFixed;
            public FixConfig config;

            public RegionFixState(FixConfig config)
            {
                FCCountFixed = 0;
                this.config = config;
            }
        }

        class RegionOptimizeState
        {
            public Int64 last04Cmd;
            public Int64 lastB6Cmd;
            public Int64 lastB7Cmd;
            public Int64 lastB9Cmd;
            public Int64 lastBACmd;
            public Int64 lastBCCmd;
            public Int64 lastF8Cmd;
            public FixConfig config;

            public int prevF2cmdAddr;
            public int prevF5cmdAddr;
            public int prevFDcmdAddr;

            public RegionOptimizeState(FixConfig config)
            {
                last04Cmd = 0;
                lastB6Cmd = 0;
                lastB7Cmd = 0;
                lastB9Cmd = 0;
                lastBACmd = 0;
                lastBCCmd = 0;
                lastF8Cmd = 0;
                this.config = config;

                prevF2cmdAddr = 0;
                prevF5cmdAddr = 0;
                prevFDcmdAddr = 0;
            }
        }

        enum VisualMapParseStateCmd
        {
            Header,
            Texture,
            Footer,
        }

        class VisualMapParseState
        {
            public VisualMapParseStateCmd state;
            public TextureDescription td;
            public UInt64 vertexLoadCmd;
            public bool isHeader;

            public VisualMapParseState()
            {
                state = VisualMapParseStateCmd.Header;
                td = new TextureDescription();
                isHeader = true;
            }
        }

        class TriangleMapParseState
        {
            public VisualMapParseStateCmd state;
            public ScrollingTextureDescription td;
            public UInt64 vertexLoadCmd;
            public bool isHeader;
            public Vertex[] vbuf;
            public Scroll[] scrollBuf;
            public bool[] scrollIsUsedBuf;
            public Int32[] vbufRomStart;
            public List<Scroll> scrolls;

            public TriangleMapParseState(List<Scroll> scrolls)
            {
                state = VisualMapParseStateCmd.Header;
                td = new ScrollingTextureDescription();
                isHeader = true;
                vbuf = new Vertex[16];
                scrollBuf = new Scroll[16];
                scrollIsUsedBuf = new bool[16];
                vbufRomStart = new Int32[16];
                this.scrolls = scrolls;
            }
        }


        public static void PerformRegionParse(ROM rom, List<Region> regions, int offset, int layer)
        {
            PerformRegionParseInternal(rom, regions, offset, layer);
        }
        static RegionParseState PerformRegionParseInternal(ROM rom, List<Region> regions, int offset, int layer)
        {
            RegionParseState state = new RegionParseState();
            rom.PushOffset(offset);
            try
            {
                int cmd = 0;
                do
                {
                    cmd = rom.Read8();
                    parser[cmd](rom, regions, state);
                    rom.AddOffset(8);
                }
                while (cmd != 0xB8);

                Region region;
                SortedRegionList graphicsData = new SortedRegionList();

                foreach (KeyValuePair<int, int> lightRegion in state.lightData.RegionList)
                {
                    //region = new Region(lightRegion.Key, lightRegion.Value, RegionState.LightData);
                    graphicsData.AddRegion(lightRegion.Key, lightRegion.Value);
                    //regions.Add(region);
                }

                // kostul
                if (state.lightData.RegionList.Count == 0)
                    graphicsData.AddRegion(rom.GetROMAddress(0x0E000000), 0x10);

                foreach (KeyValuePair<int, int> textureRegion in state.textureData.RegionList)
                {
                    //region = new Region(textureRegion.Key, textureRegion.Value, RegionState.TextureInfo);
                    graphicsData.AddRegion(textureRegion.Key, textureRegion.Value);
                    //regions.Add(region);
                }

                foreach (KeyValuePair<int, int> vertexRegion in state.vertexData.RegionList)
                {
                    //region = new Region(vertexRegion.Key, vertexRegion.Value, RegionState.VertexInfo);
                    graphicsData.AddRegion(vertexRegion.Key, vertexRegion.Value);
                    //regions.Add(region);
                }

                int count = 0;
                foreach (KeyValuePair<int, int> notFixedRegion in graphicsData.RegionList)
                {
                    region = new DynamicRegion(notFixedRegion.Key, notFixedRegion.Value, RegionState.GraphicsData);
                    region.number = count++;
                    regions.Add(region);
                }


                region = new DisplayListRegion(offset, rom.offset - offset, state.isFogEnabled, state.isEnvColorEnabled, state.FCcmd, layer, state.isUnusedTrimmingAllowed);
                regions.Add(region);
            }
            finally
            {
                rom.PopOffset();
            }
            return state;
        }
        public static void PerformRegionFix(ROM rom, Region region, FixConfig config)
        {
            RegionFixState state = new RegionFixState(config);
            DisplayListRegion dlRegion = (DisplayListRegion)region;

            rom.PushOffset(region.romStart);
            byte curCmdIndex;
            do
            {
                curCmdIndex = rom.Read8();
                FixParseCmd func = fixParser[curCmdIndex];
                func(rom, dlRegion, state);
                rom.AddOffset(8);
            }
            while (rom.offset < region.romStart + region.length);
            rom.PopOffset();
            rom.ReadData(region.romStart, region.length, region.data);
        }
        public static void PerformRegionOptimize(ROM realRom, Region region, FixConfig config)
        {
            // This is fake rom but it works anyways, just more convenient
            // Want to be safe with overwriting the whole display list
            ROM fakeRom = new ROM(region.data);
            RegionOptimizeState state = new RegionOptimizeState(config);

            DisplayListRegion dlRegion = (DisplayListRegion)region;

            byte curCmdIndex;
            do
            {
                curCmdIndex = fakeRom.Read8();
                OptimizeParserCmd func = optimizeParser[curCmdIndex];
                func(fakeRom, dlRegion, state);
                fakeRom.AddOffset(8);
            }
            while (fakeRom.offset < region.length);

            // Now write data to real rom + trimming
            // bzero
            fakeRom.offset = 0;
            realRom.PushOffset(region.romStart);
            {
                do
                {
                    realRom.Write64(0x0101010101010101);
                    realRom.AddOffset(8);
                    fakeRom.AddOffset(8);
                } while (fakeRom.offset < region.length);
            }
            realRom.PopOffset();

            fakeRom.offset = 0;
            realRom.PushOffset(region.romStart);
            {
                int start = region.romStart;
                do
                {
                    Int64 cmd = fakeRom.Read64();
                    fakeRom.AddOffset(8);

                    if (config.trimNOPs && cmd == 0 && dlRegion.isUnusedTrimmingAllowed)
                        continue;

                    realRom.Write64((ulong)cmd);
                    realRom.AddOffset(8);
                } while (fakeRom.offset < region.length);

                region.length = realRom.offset - start;
                region.data = new byte[region.length];
                realRom.ReadData(region.romStart, region.length, region.data);
            }
            realRom.PopOffset();
        }

        public static void PerformVisualMapRebuild(ROM realRom, Region region, int maxDLLength)
        {
            // This is fake rom but it works anyways, just more convenient
            // Want to be safe with overwriting the whole display list
            ROM fakeRom = new ROM(region.data);
            VisualMapParseState state = new VisualMapParseState();

            DisplayListRegion dlRegion = (DisplayListRegion)region;
            VisualMap map = new VisualMap();

            byte curCmdIndex;
            do
            {
                curCmdIndex = fakeRom.Read8();
                VisualMapParserCmd func = visualMapParser[curCmdIndex];
                func(fakeRom, map, state);
                fakeRom.AddOffset(8);
            }
            while (fakeRom.offset < region.length);

            ROM visualMapROM = new ROM(new byte[maxDLLength]);
            int visualMapLength = map.MakeF3D(visualMapROM);
            

            // Now write data to real rom + trimming
            // bzero
            fakeRom.offset = 0;
            realRom.PushOffset(region.romStart);
            {
                do
                {
                    realRom.Write64(0x0101010101010101);
                    realRom.AddOffset(8);
                    fakeRom.AddOffset(8);
                } while (fakeRom.offset < region.length);
            }
            realRom.PopOffset();

            visualMapROM.offset = 0;
            realRom.PushOffset(region.romStart);
            {
                int start = region.romStart;
                do
                {
                    Int64 cmd = visualMapROM.Read64();
                    visualMapROM.AddOffset(8);

                    realRom.Write64((ulong)cmd);
                    realRom.AddOffset(8);
                } while (visualMapROM.offset < visualMapLength);

                region.length = realRom.offset - start;
                region.data = new byte[region.length];
                realRom.ReadData(region.romStart, region.length, region.data);
            }
            realRom.PopOffset();
        }

        public static void PerformTriangleMapRebuild(ROM realRom, Region region, int maxDLLength, List<Scroll> scrolls)
        {
            TriangleMapParseState state = new TriangleMapParseState(scrolls);

            DisplayListRegion dlRegion = (DisplayListRegion)region;
            TriangleMap map = new TriangleMap();

            realRom.PushOffset(region.romStart);
            byte curCmdIndex;
            do
            {
                curCmdIndex = realRom.Read8();
                TriangleMapParserCmd func = triangleMapParser[curCmdIndex];
                func(realRom, map, state);
                realRom.AddOffset(8);
            }
            while (realRom.offset < region.romStart + region.length);
            realRom.PopOffset();

            ROM fakeRom = (ROM)realRom.Clone();

            // bzero
            fakeRom.PushOffset(region.romStart);
            {
                do
                {
                    fakeRom.Write64(0x0101010101010101);
                    fakeRom.AddOffset(8);
                } while (fakeRom.offset < region.romStart + region.length);
            }
            fakeRom.PopOffset();

            fakeRom.offset = region.romStart;
            int triangleMapLength = map.MakeF3D(fakeRom);
            if (triangleMapLength > maxDLLength)
                throw new OutOfMemoryException("No memory for DL available :(");

            realRom.TransferFrom(fakeRom);

            realRom.offset = fakeRom.offset;
            region.length = realRom.offset - region.romStart;
            region.data = new byte[region.length];
            realRom.ReadData(region.romStart, region.length, region.data);
        }



        private static void RegionParse_common(ROM rom, List<Region> regions, RegionParseState state) { }
        private static void FixParse_common(ROM rom, DisplayListRegion region, RegionFixState state) { }
        private static void OptimizeParse_common(ROM rom, DisplayListRegion region, RegionOptimizeState state) { }
        private static void VisualMapParse_common(ROM rom, VisualMap map, VisualMapParseState state)
        {
            switch(state.state)
            {
                case VisualMapParseStateCmd.Header:
                    map.AddHeaderCmd((UInt64)rom.Read64());
                    break;
                case VisualMapParseStateCmd.Texture:
                    state.td.Add((UInt64)rom.Read64());
                    break;
                case VisualMapParseStateCmd.Footer:
                    map.AddFooterCmd((UInt64)rom.Read64());
                    break;
            }
        }
        private static void TriangleMapParse_common(ROM rom, TriangleMap map, TriangleMapParseState state)
        {
            switch (state.state)
            {
                case VisualMapParseStateCmd.Header:
                    map.AddHeaderCmd((UInt64)rom.Read64());
                    break;
                case VisualMapParseStateCmd.Texture:
                    state.td.Add((UInt64)rom.Read64());
                    break;
                case VisualMapParseStateCmd.Footer:
                    map.AddFooterCmd((UInt64)rom.Read64());
                    break;
            }
        }



        private static void RegionParse_cmd01(ROM rom, List<Region> regions, RegionParseState state)
        {
            throw new NotSupportedException("segmented vectors not supported");
        }

        private static void RegionParse_cmd03(ROM rom, List<Region> regions, RegionParseState state)
        {
            int segment = rom.Read8(4);
            if (segment != 0x0e)
                return;

            int segmentedAddress = rom.Read32(4);
            int address = rom.GetROMAddress(segmentedAddress);

            state.lightData.AddRegion(address, 0x8);
        }

        private static void RegionParse_cmd04(ROM rom, List<Region> regions, RegionParseState state)
        {
            int segment = rom.Read8(4);
            if (segment != 0x0e)
                return;

            int segmentedAddress = rom.Read32(4);
            int address = rom.GetROMAddress(segmentedAddress);
            state.vertexData.AddRegion(address, rom.Read16(2));
            //regions.Add(new Region(address, rom.Read16(2), RegionState.VertexInfo));
        }

        private static void RegionParse_cmdB7(ROM rom, List<Region> regions, RegionParseState state)
        {
            if ((rom.Read32(4) & 0x00010000) != 0)
                state.isFogEnabled = true;
        }

        private static void RegionParse_cmdB9(ROM rom, List<Region> regions, RegionParseState state)
        {
            if (state.B9cmdfirst == 0)
                state.B9cmdfirst = rom.Read64();
        }

        private static void RegionParse_cmdF0(ROM rom, List<Region> regions, RegionParseState state)
        {
            if (state.FDAddress == -1)
                return;

            int w0 = rom.Read16(5);
            int colorCount = (w0 >> (4 + 2)) + 1;
            
            state.textureData.AddRegion(state.FDAddress, colorCount * 2);
        }

        private static void RegionParse_cmdF3(ROM rom, List<Region> regions, RegionParseState state)
        {
            if (state.FDAddress == -1)
                return;

            int w0 = (int) rom.Read32(0);
            int w1 = (int) rom.Read32(4);
            int uls = (w0 >> 12) & 0x3FF;
            int ult = w0 & 0x3FF;
            int lrs = (w1 >> 12) & 0x3FF;
            int dxt = w1 & 0x3FF;

            // idk how this actually works
            int textureSize = (lrs + 1) * 4;
            state.textureData.AddRegion(state.FDAddress, textureSize);
        }

        private static void RegionParse_cmdFB(ROM rom, List<Region> regions, RegionParseState state)
        {
            state.isEnvColorEnabled = true;
        }

        private static void RegionParse_cmdFC(ROM rom, List<Region> regions, RegionParseState state)
        {
            if (state.FCcmd == -1)
                state.FCcmd = rom.Read64();
            state.FCCount++;
        }

        private static void RegionParse_cmdFD(ROM rom, List<Region> regions, RegionParseState state)
        {
            state.FDAddress = -1;

            int segment = rom.Read8(4);
            if (segment != 0x0e)
                return;

            int segmentedAddress = rom.Read32(4);
            int address = (int)rom.GetROMAddress(segmentedAddress);
            state.FDAddress = address;
        }



        private static void VisualMapParse_cmd04(ROM rom, VisualMap map, VisualMapParseState state)
        {
            state.vertexLoadCmd = (UInt64) rom.Read64();
        }
        private static void VisualMapParse_cmdBB(ROM rom, VisualMap map, VisualMapParseState state)
        {
            if ((UInt64)rom.Read64() == 0xBB000000FFFFFFFF)
                state.state = VisualMapParseStateCmd.Footer;

            VisualMapParse_common(rom, map, state);
        }
        private static void VisualMapParse_cmdBF(ROM rom, VisualMap map, VisualMapParseState state)
        {
            state.isHeader = false;
            state.state = VisualMapParseStateCmd.Footer;
            map.AddTriangle(state.td, state.vertexLoadCmd, (UInt64) rom.Read64());
        }
        private static void VisualMapParse_cmdF2(ROM rom, VisualMap map, VisualMapParseState state)
        {
            VisualMapParse_common(rom, map, state);
            state.state = state.isHeader ? VisualMapParseStateCmd.Header : VisualMapParseStateCmd.Footer; // Case for fog
        }
        private static void VisualMapParse_cmdFB(ROM rom, VisualMap map, VisualMapParseState state)
        {
            // Some importers have the only EnvColor func for everything lmfao
            if (rom.Read8(8) != 0xFD)
                goto fini;

            state.state = VisualMapParseStateCmd.Texture;
            state.td = new TextureDescription();

fini:
            VisualMapParse_common(rom, map, state);
        }

        private static void VisualMapParse_cmdFD(ROM rom, VisualMap map, VisualMapParseState state)
        {
            UInt64 fdCmd = state.td.GetTextureCMD();
            if (state.state != VisualMapParseStateCmd.Texture)
            {
                state.state = VisualMapParseStateCmd.Texture;
                state.td = new TextureDescription();
            }
            VisualMapParse_common(rom, map, state);
        }



        private static void TriangleMapParse_cmd04(ROM rom, TriangleMap map, TriangleMapParseState state)
        {
            state.vertexLoadCmd = (UInt64)rom.Read64();
            byte vertexDesc = rom.Read8(1);

            byte vertexCount = (byte) (((vertexDesc & 0xF0) >> 4) + 1);
            byte vertexOffset = (byte)((vertexDesc & 0x0F));
            Int32 segmentedAddress = rom.Read32(4);

            Int32 romPtr = rom.GetROMAddress(segmentedAddress);
            if (romPtr == -1)
                throw new ArgumentException("Invalid segmented address!");

            Int32 romPtrEnd = romPtr + vertexCount * 0x10;
            segmentedAddress += vertexCount * 0x10;

            rom.PushOffset(romPtr);
            for (int vertex = vertexOffset; vertex < vertexCount; vertex++)
            {
                Int64 lo = rom.Read64();
                Int64 hi = rom.Read64(8);

                state.vbuf[vertex] = new Vertex((UInt64)lo, (UInt64)hi);
                state.vbufRomStart[vertex] = rom.offset;
                state.scrollBuf[vertex] = state.scrolls.Find(s => s.SegmentedAddress <= segmentedAddress && segmentedAddress < s.SegmentedAddress + s.VertexCount * 0x10);
                state.scrollIsUsedBuf[vertex] = false;

                rom.AddOffset(0x10);
                segmentedAddress += 0x10;
            }
            rom.PopOffset();
        }

        private static void TriangleMapParse_cmdBB(ROM rom, TriangleMap map, TriangleMapParseState state)
        {
            if ((UInt64)rom.Read64() == 0xBB000000FFFFFFFF)
                state.state = VisualMapParseStateCmd.Footer;

            TriangleMapParse_common(rom, map, state);
        }

        private static void TriangleMapParse_cmdBF(ROM rom, TriangleMap map, TriangleMapParseState state)
        {
            state.isHeader = false;
            state.state = VisualMapParseStateCmd.Footer;

            byte v0index = (byte) (rom.Read8(5) / 0xA);
            byte v1index = (byte) (rom.Read8(6) / 0xA);
            byte v2index = (byte) (rom.Read8(7) / 0xA);

            map.AddVertexRegion(new DynamicRegion(state.vbufRomStart[v0index], 0x10, RegionState.GraphicsData));
            map.AddVertexRegion(new DynamicRegion(state.vbufRomStart[v1index], 0x10, RegionState.GraphicsData));
            map.AddVertexRegion(new DynamicRegion(state.vbufRomStart[v2index], 0x10, RegionState.GraphicsData));

            // This assumes all scrolls are scrolling at the same speed which is usually true :3
            if (state.scrollBuf[v0index] != state.td.scroll ||
               (state.scrollBuf[v0index] != null) && (state.td.scroll == null))
            {
                ScrollingTextureDescription oldTd = state.td;

                state.td = new ScrollingTextureDescription();
                state.td.AddRange(oldTd);
                state.td.scroll = state.scrollBuf[v0index];
            }
            else
            {
                Console.Write("a");
            }

            state.scrollIsUsedBuf[v0index] = true;
            if (state.scrollBuf[v0index] == state.scrollBuf[v1index])
                state.scrollIsUsedBuf[v1index] = true;
            else
                Console.Write("a");

            if (state.scrollBuf[v0index] == state.scrollBuf[v2index])
                state.scrollIsUsedBuf[v2index] = true;
            else
                Console.Write("b");

            map.AddTriangle(state.td, state.vbuf[v0index], state.vbuf[v1index], state.vbuf[v2index]);
        }

        private static void TriangleMapParse_cmdF2(ROM rom, TriangleMap map, TriangleMapParseState state)
        {
            TriangleMapParse_common(rom, map, state);
            state.state = state.isHeader ? VisualMapParseStateCmd.Header : VisualMapParseStateCmd.Footer; // Case for fog
        }

        private static void TriangleMapParse_cmdFB(ROM rom, TriangleMap map, TriangleMapParseState state)
        {
            // Some importers have the only EnvColor func for everything lmfao
            if (rom.Read8(8) != 0xFD)
                goto fini;

            state.state = VisualMapParseStateCmd.Texture;
            state.td = new ScrollingTextureDescription();

        fini:
            TriangleMapParse_common(rom, map, state);
        }

        private static void TriangleMapParse_cmdFD(ROM rom, TriangleMap map, TriangleMapParseState state)
        {
            UInt64 fdCmd = state.td.GetTextureCMD();
            if (state.state != VisualMapParseStateCmd.Texture)
            {
                // Make sure scroll cannot appear on change of scrolling texture
                for (int i = 0; i < 16; i++)
                {
                    if (state.scrollIsUsedBuf[i])
                    {
                        Scroll scroll = state.scrollBuf[i];
                        state.scrolls.Remove(scroll);
                    }
                }

                /*for (int i = 0; i < state.scrollBuf.Length; i++)
                {
                    state.scrollBuf[i] = null;
                    state.scrollIsUsedBuf[i] = false;
                }*/

                state.state = VisualMapParseStateCmd.Texture;
                state.td = new ScrollingTextureDescription();
            }
            TriangleMapParse_common(rom, map, state);
        }




        private static void OptimizeParse_cmd04(ROM rom, DisplayListRegion region, RegionOptimizeState state)
        {
            // initial state
            if (state.last04Cmd == 0)
            {
                state.last04Cmd = rom.Read64();
                return;
            }

            Int64 cmd = rom.Read64();

            // if current 04 loads the same vertices, remove current cmd
            if (cmd == state.last04Cmd)
            {
                rom.Write64(0);
                return;
            }

            // new vertices are being loaded, update the thing
            state.last04Cmd = cmd;

            // if previous cmd is 0x04, it will be overriden by current 04 anyways
            if (rom.Read8(-8) == 0x04)
            {
                rom.Write64(0, -8);
            }
        }
        private static void OptimizeParse_cmdBA(ROM rom, DisplayListRegion region, RegionOptimizeState state)
        {
            Int64 cmd = rom.Read64();
            // initial state
            if (state.lastBACmd == 0)
            {
                state.lastBACmd = cmd;
                return;
            }

            // if current cmd loads the same, remove current cmd
            if (cmd == state.lastBACmd)
            {
                rom.Write64(0);
                return;
            }
        }
        private static void OptimizeParse_cmdF8(ROM rom, DisplayListRegion region, RegionOptimizeState state)
        {
            Int64 cmd = rom.Read64();
            // initial state
            if (state.lastF8Cmd == 0)
            {
                state.lastF8Cmd = cmd;
                return;
            }

            // if current cmd loads the same, remove current cmd
            if (cmd == state.lastF8Cmd)
            {
                rom.Write64(0);
                return;
            }
        }
        private static void OptimizeParse_cmdB6(ROM rom, DisplayListRegion region, RegionOptimizeState state)
        {
            // Very failsafe approach
            state.lastB7Cmd = 0;

            Int64 cmd = rom.Read64();
            // Basically NOP
            if ((UInt64)cmd == 0xB600000000000000)
            {
                rom.Write64(0);
                return;
            }

            // initial state
            if (state.lastB6Cmd == 0)
            {
                state.lastB6Cmd = cmd;
                return;
            }

            // if current cmd removes the same, remove current cmd
            if (cmd == state.lastB6Cmd)
            {
                rom.Write64(0);
                return;
            }
        }
    
        private static void OptimizeParse_cmdB7(ROM rom, DisplayListRegion region, RegionOptimizeState state)
        {
            // Very failsafe approach
            state.lastB6Cmd = 0;

            Int64 cmd = rom.Read64();
            // Basically NOP
            if ((UInt64)cmd == 0xB700000000000000)
            {
                rom.Write64(0);
                return;
            }


            // initial state
            if (state.lastB7Cmd == 0)
            {
                state.lastB7Cmd = cmd;
                return;
            }

            // if current cmd loads the same, remove current cmd
            if (cmd == state.lastB7Cmd)
            {
                rom.Write64(0);
                return;
            }
        }
        private static void OptimizeParse_cmdB9(ROM rom, DisplayListRegion region, RegionOptimizeState state)
        {
            Int64 cmd = rom.Read64();
            // initial state
            if (state.lastB9Cmd == 0)
            {
                state.lastB9Cmd = cmd;
                return;
            }

            // if current cmd loads the same, remove current cmd
            if (cmd == state.lastB9Cmd)
            {
                rom.Write64(0);
                return;
            }
        }

        private static void OptimizeParse_cmdBC(ROM rom, DisplayListRegion region, RegionOptimizeState state)
        {
            Int64 cmd = rom.Read64();
            // initial state
            if (state.lastBCCmd == 0)
            {
                state.lastBCCmd = cmd;
                return;
            }

            // if current cmd loads the same, remove current cmd
            if (cmd == state.lastBCCmd)
            {
                rom.Write64(0);
                return;
            }
        }

        private static void OptimizeParse_cmdBF(ROM rom, DisplayListRegion region, RegionOptimizeState state)
        {
            state.prevF2cmdAddr = 0;
            state.prevF5cmdAddr = 0;
            state.prevFDcmdAddr = 0;
        }

        private static void OptimizeParse_cmdF2(ROM rom, DisplayListRegion region, RegionOptimizeState state)
        {
            if (state.prevF2cmdAddr != 0)
            {
                rom.PushOffset(state.prevF2cmdAddr);
                rom.Write64(0);
                rom.PopOffset();
            }

            state.prevF2cmdAddr = rom.offset;
        }

        private static void OptimizeParse_cmdF5(ROM rom, DisplayListRegion region, RegionOptimizeState state)
        {
            if (state.prevF5cmdAddr != 0)
            {
                rom.PushOffset(state.prevF5cmdAddr);
                rom.Write64(0);
                rom.PopOffset();
            }

            state.prevF5cmdAddr = rom.offset;
        }

        private static void OptimizeParse_cmdFD(ROM rom, DisplayListRegion region, RegionOptimizeState state)
        {
            if (state.prevFDcmdAddr != 0)
            {
                rom.PushOffset(state.prevFDcmdAddr);
                rom.Write64(0);
                rom.PopOffset();
            }

            state.prevFDcmdAddr = rom.offset;
        }

        private static void FixParse_cmdB6(ROM rom, DisplayListRegion region, RegionFixState state)
        {
            if (state.config.disableFog)
                rom.Write64(0);
        }


        private static void FixParse_cmdB7(ROM rom, DisplayListRegion region, RegionFixState state)
        {
            if (state.config.disableFog)
                    rom.Write64(0);
        }

        private static void FixParse_cmdB9(ROM rom, DisplayListRegion region, RegionFixState state)
        {
            if (state.config.disableFog)
            {
                rom.Write64(0);
                return;
            }

            if (!state.config.fixOtherMode)
                return;

            if (!region.isFogEnabled)
            {
                return;
            }

            if ((ulong)rom.Read64(-8) != 0xBA00140200000000 || (ulong)rom.Read64(8) != 0xB600000000010000)
                return;

            UInt64 B9Cmd = OtherMode.GetB9Cmd(region.layer);
            if (B9Cmd != 0)
                rom.Write64(B9Cmd);
        }


        private static void FixParse_cmdBA(ROM rom, DisplayListRegion region, RegionFixState state)
        {
            if (state.config.disableFog)
                rom.Write64(0);
        }

        private static void FixParse_cmdBC(ROM rom, DisplayListRegion region, RegionFixState state)
        {
            if (state.config.disableFog)
            {
                rom.Write64(0);
                return;
            }

            if (state.config.nerfFog)
            {
                float A = rom.Read16(4);
                float B = rom.Read16(6);

                float min = 500 * (1 - B / A);
                float max = 128000 / A + min;

                // nerf fog
                min += 5;
                A = 128000 / (max - min);
                B = (500 - min) * 256 / (max - min);

                int Aint = (int)A;
                int Bint = (int)B;

                rom.Write16(Aint, 4);
                rom.Write16(Bint, 6);
            }
        }
        
        private static void FixParse_cmdF8(ROM rom, DisplayListRegion region, RegionFixState state)
        {
            if (state.config.disableFog)
                rom.Write64(0);
        }

        private static void FixParse_cmdFC(ROM rom, DisplayListRegion region, RegionFixState state)
        {
            if (!state.config.fixCombiners)
                return;

            if (state.FCCountFixed != 0)
                return;

            CombinerCommand cmd = CombinerCommand.GetNewCombiner(region);
            UInt64 FCCmd = cmd.GetFCcmd();
            if (FCCmd != 0)
            {
                state.FCCountFixed++;
                rom.Write64(FCCmd);
            }
        }
    }
}
