using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LevelCombiner
{
    public class EditorScroll : ScrollObject
    {
        public EditorScroll(ROM rom) : base(rom)
        {
            segmentedAddress = 0x0e000000;
            acts = rom.Read8(0x02);
            X = rom.Read16(0x04);
            Y = rom.Read16(0x06);
            Z = rom.Read16(0x08);
            BParam = rom.Read16(0x10);
            BParam2 = rom.Read16(0x12);
        }

        public EditorScroll(int vertexCount, int segmentedAddress, byte speed, byte acts, TextureAxis axis, int behav, int offset) : base(vertexCount, segmentedAddress, speed, acts, axis, offset, behav)
        { }

        public short X
        {
            get
            {
                byte xval = (byte) (((segmentedAddress >> 16) & 0xff) + 2);
                return MakeHiddenFloat(xval);
            }
            set
            {
                byte xval = (byte) (GetHiddenFloatByte(value) - 2);
                segmentedAddress = (int) ((segmentedAddress & 0xff00ffff) + (xval << 16));
            }
        }

        public short Y
        {
            get
            {
                return 0; // MakeHiddenFloat((byte) (vertexCount / 3));
            }
            set
            {
                if (value != 0)
                    vertexCount = GetHiddenFloatByte(value) * 3;
            }
        }

        public short BParam2
        {
            get
            {
                return (short) vertexCount;
            }
            set
            {
                if (value != 0)
                    vertexCount = value;
            }
        }

        public short Z
        {
            get
            {
                return MakeHiddenFloat(speed);
            }
            set
            {
                speed = GetHiddenFloatByte(value);
            }
        }

        public short BParam
        {
            get
            {
                int axisOffset = 0;
                switch (axis)
                {
                    case TextureAxis.X:
                        axisOffset = 0x8;
                        break;
                    case TextureAxis.Y:
                        axisOffset = 0xA;
                        break;
                }

                return (short) ((segmentedAddress & 0xffff) + axisOffset);
            }
            set
            {
                int vertexOffset = value & 0xfff0;
                int axisOffset = value & 0xf;

                if (axisOffset == 0x8)
                {
                    axis = TextureAxis.X;
                }
                else if (axisOffset == 0xa)
                {
                    axis = TextureAxis.Y;
                }
                else
                {
                    throw new Exception("Editor scroll axis offset is wrong!\n");
                }

                segmentedAddress = (int)(segmentedAddress & 0xffff0000) + vertexOffset;
            }
        }


        byte GetHiddenFloatByte(short A)
        {
            float floatA = A;
            byte[] data = BitConverter.GetBytes(floatA);
            return data[2];
        }

        short MakeHiddenFloat(byte val)
        {
            byte[] data = new byte[4];
            data[2] = val;
            data[3] = 0x46;
            float fl = BitConverter.ToSingle(data, 0);
            return Convert.ToInt16(fl);
        }

        public override string ToString()
        {
            return segmentedAddress.ToString("X8") + " " + vertexCount.ToString();
        }

        public override void WriteScroll(ROM rom)
        {
            rom.PushOffset(romOffset);
            rom.Write8(acts, 0x02);
            rom.Write16(X, 0x04);
            rom.Write16(Y, 0x06);
            rom.Write16(Z, 0x08);
            rom.Write16(BParam, 0x10);
            rom.Write16(BParam2, 0x12);
            rom.Write32(behavior, 0x14);
            rom.PopOffset();
        }
    }

    public class TextureScroll : Scroll
    {

    }

    public abstract class ScrollObject : Scroll
    {
        public int romOffset;
        public int behavior;

        public ScrollObject(ROM rom)
        {
            romOffset = rom.offset;
            behavior = rom.Read32(0x14);
        }

        public ScrollObject(int vertexCount, int segmentedAddress, byte speed, byte acts, TextureAxis axis, int romOffset, int behavior) : base(vertexCount, segmentedAddress, speed, acts, axis)
        {
            this.romOffset = romOffset;
            this.behavior = behavior;
        }

        public abstract void WriteScroll(ROM rom);

        public void Disable(ROM rom)
        {
            rom.PushOffset(romOffset);
            rom.Write8(0, 0x02);
            rom.PopOffset();
        }
    }

    public enum TextureAxis
    {
        X,
        Y,
    };

    public class Scroll : ScrollDesc
    {
        public int vertexCount;
        public int segmentedAddress;

        public Scroll(int vertexCount, int segmentedAddress, byte speed, byte acts, TextureAxis axis) : base(speed, acts, axis)
        {
            this.vertexCount = vertexCount;
            this.segmentedAddress = segmentedAddress;
        }

        public Scroll() { }
    }

    public class ScrollDesc : IEquatable<ScrollDesc>
    {
        public byte speed;
        public byte acts;
        public TextureAxis axis;

        public ScrollDesc(byte speed, byte acts, TextureAxis axis) 
        {
            this.speed = speed;
            this.acts = acts;
            this.axis = axis;
        }

        public ScrollDesc() { }

        public override int GetHashCode()
        {
            return speed ^ acts ^ (int)axis;
        }

        public bool Equals(ScrollDesc other)
        {
            if (this == null || other == null)
                return false;

            return speed == other.speed && acts == other.acts && axis == other.axis;
        }
    }
}
