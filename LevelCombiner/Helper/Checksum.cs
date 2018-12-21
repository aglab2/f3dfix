using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LevelCombiner
{
    class Checksum
    {
        const int CHECKSUM_START = 0x1000;
        const int CHECKSUM_LENGTH = 0x100000;
        const int CHECKSUM_HEADERPOS = 0x10;
        const int CHECKSUM_END = (CHECKSUM_START + CHECKSUM_LENGTH);

        const uint CHECKSUM_STARTVALUE = 0xf8ca4ddc;

        private static uint ROL(uint i, int b)
        {
            return (((i) << (b)) | ((i) >> (32 - (b))));
        }

        private static uint BYTES2LONG(PositionalBuffer pb)
        {
            return (uint)((pb.Read8(0)  << 24) | ((pb.Read8(1) & 0xff) << 16) | ((pb.Read8(2) & 0xff) << 8) | ((pb.Read8(3) & 0xff)));
        }

        private static void LONG2BYTES(uint l, PositionalBuffer pb)
        {
            pb.Write8((byte)(l >> 24), 0);
            pb.Write8((byte)(l >> 16), 1);
            pb.Write8((byte)(l >> 8) , 2);
            pb.Write8((byte)(l)      , 3);
        }

    
        public static void CalculateChecksum(PositionalBuffer pb)
        {
            uint sum1, sum2;

            {
                uint i;
                uint c1, k1, k2;
                uint t1, t2, t3, t4;
                uint t5, t6;

                t1 = CHECKSUM_STARTVALUE;
                t2 = CHECKSUM_STARTVALUE;
                t3 = CHECKSUM_STARTVALUE;
                t4 = CHECKSUM_STARTVALUE;
                t5 = CHECKSUM_STARTVALUE;
                t6 = CHECKSUM_STARTVALUE;

                pb.offset = CHECKSUM_START;

                for (i = 0; i < CHECKSUM_LENGTH; i += 4)
                {
                    c1 = BYTES2LONG(pb);
                    k1 = t6 + c1;
                    if (k1 < t6) t4++;
                    t6 = k1;
                    t3 ^= c1;
                    k2 = c1 & 0x1f;
                    k1 = ROL(c1, (int)k2);
                    t5 += k1;
                    if (c1 < t2)
                    {
                        t2 ^= k1;
                    }
                    else
                    {
                        t2 ^= t6 ^ c1;
                    }
                    t1 += c1 ^ t5;
                    pb.AddOffset(4);
                }
                sum1 = t6 ^ t4 ^ t3;
                sum2 = t5 ^ t2 ^ t1;
            }
            pb.offset = 16;
            LONG2BYTES(sum1, pb);
            pb.offset = 20;
            LONG2BYTES(sum2, pb);
        }
    }
}
