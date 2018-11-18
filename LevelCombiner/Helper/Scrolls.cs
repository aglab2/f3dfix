using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LevelCombiner
{
    public class Scroll : IEquatable<Scroll>, IComparable<Scroll>, ICloneable
    {
        public int SegmentedAddress
        {
            get
            {
                int address = 0;

                byte hiddenX = GetHiddenFloatByte(X);

                address += hiddenX * 0x10000;
                address += bparam;

                return address - 0x20000 + 0x0E000000 - axisOffset;
            }
            set
            {
                int newAddress = value - 0x0E000000 + 0x20000 + axisOffset;

                X = SetHiddenFloatByte(X, (byte)((newAddress >> 16) & 0xFF));
                bparam = (ushort)(newAddress & 0xFFFF);
            }
        }

        public int VertexCount
        {
            get
            {
                return GetHiddenFloatByte(Y) * 3;
            }
            set
            {
                Y = SetHiddenFloatByte(Y, (byte) (value / 3));
            }
        }

        public override string ToString()
        {
            return SegmentedAddress.ToString() + " " + VertexCount.ToString();
        }

        public override int GetHashCode()
        {
            return X ^ Y ^ Z ^ bparam ^ acts ^ romOffset;
        }

        public bool Equals(Scroll other)
        {
            if (other == null)
                return false;

            return romOffset == other.romOffset && X == other.X && Y == other.Y && Z == other.Z && bparam == other.bparam && acts == other.acts;
        }

        public int CompareTo(Scroll other)
        {
            return romOffset.CompareTo(other.romOffset);
        }

        private ushort bparam;
        private int behaviour;
        private int X, Y, Z;
        private byte acts;
        private int romOffset;
        private int axisOffset; // this is purely magic

        byte GetHiddenFloatByte(int A)
        {
            float floatA = A;
            byte[] data = BitConverter.GetBytes(floatA);
            return data[2];
        }

        int SetHiddenFloatByte(int A, byte v)
        {
            float floatA = A;
            byte[] data = BitConverter.GetBytes(floatA);
            data[2] = v;
            float newFloatA = BitConverter.ToSingle(data, 0);
            return Convert.ToInt16(newFloatA);
        }

        public Scroll() { }

        public Scroll(ROM rom)
        {
            acts      = rom.Read8 (0x02);
            X         = rom.Read16(0x04);
            Y         = rom.Read16(0x06);
            Z         = rom.Read16(0x08);
            bparam    = (ushort) rom.Read16(0x10);
            behaviour = rom.Read32(0x14);
            romOffset = rom.offset;

            // To get axis affset, get the lower bits of SegmentedAddress
            // On next calls, SegmentedAddress with have axisOffset removed
            axisOffset = 0;
            axisOffset = SegmentedAddress & 0xF;
        }

        public void WriteScroll(ROM rom)
        {
            rom.PushOffset(romOffset);
            rom.Write8(acts,       0x02);
            rom.Write16(X,         0x04);
            rom.Write16(Y,         0x06);
            rom.Write16(Z,         0x08);
            rom.Write16(bparam,    0x10);
            rom.Write32(behaviour, 0x14);
            rom.PopOffset();
        }

        public object Clone()
        {
            Scroll obj = new Scroll
            {
                X = X,
                Y = Y,
                Z = Z,
                romOffset = romOffset,
                bparam = bparam,
                behaviour = behaviour,
                acts = acts,
                axisOffset = axisOffset,
            };
            return obj;
        }
    }
}
