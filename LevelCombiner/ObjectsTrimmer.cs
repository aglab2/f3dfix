using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LevelCombiner
{
    class ObjectsTrimmer
    {
        static byte[] emptyObject = { 0x24, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x13, 0x00, 0x00, 0x00 };

        public static void Trim(Region region, bool addObjects)
        {
            ROM rom = new ROM(region.data);

            MemoryStream trimmedObject = new MemoryStream();

            while (rom.offset < region.length)
            {
                byte curCmdIndex = rom.Read8();
                byte curCmdSize = rom.Read8(1);
                
                if (curCmdIndex == 0x24)
                {
                    byte acts = rom.Read8(2);
                    if (acts == 0)
                        goto fini;
                }


                byte[] data = new byte[curCmdSize];
                rom.ReadData(rom.offset, curCmdSize, data);
                trimmedObject.Write(data, 0, curCmdSize);

fini:
                rom.AddOffset(curCmdSize);
            }

            if (addObjects)
            {
                trimmedObject.Write(emptyObject, 0, emptyObject.Count());
                trimmedObject.Write(emptyObject, 0, emptyObject.Count());
                trimmedObject.Write(emptyObject, 0, emptyObject.Count());
                trimmedObject.Write(emptyObject, 0, emptyObject.Count());
                trimmedObject.Write(emptyObject, 0, emptyObject.Count());
            }

            region.data = trimmedObject.ToArray();
            region.length = (int) trimmedObject.Length;

            trimmedObject.Dispose();
        }
    }
}
