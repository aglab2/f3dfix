using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LevelCombiner
{
    public class HiddenFloatByte
    {
        public static byte Get(short A)
        {
            float floatA = A;
            byte[] data = BitConverter.GetBytes(floatA);
            return data[2];
        }

        public static short Make(byte val)
        {
            byte[] data = new byte[4];
            data[2] = val;
            data[3] = 0x46;
            float fl = BitConverter.ToSingle(data, 0);
            return Convert.ToInt16(fl);
        }
    }

    public class EditorScroll : ScrollObject
    {
        public int vertexCount;
        public int segmentedAddress;
        public bool isClassic;

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

        public EditorScroll(int vertexCount, int segmentedAddress, byte speed, byte acts, TextureAxis axis, int behav, int offset) : base(speed, acts, axis, offset, behav)
        {
            this.vertexCount = vertexCount;
            this.segmentedAddress = segmentedAddress;
        }

        public short X
        {
            // editor classic 0x8042 - hence -2
            // editor 2.2 0x8065
            get
            {
                byte xval = (byte) (((segmentedAddress >> 16) & 0xff) + (isClassic ? 0x2 : 0x65));
                return HiddenFloatByte.Make(xval);
            }
            set
            {
                byte hiddenByte = HiddenFloatByte.Get(value);
                isClassic = hiddenByte < 0x65;
                byte xval = (byte) (hiddenByte - (isClassic ? 0x2 : 0x65));
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
                    vertexCount = HiddenFloatByte.Get(value) * 3;
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
                return HiddenFloatByte.Make(speed);
            }
            set
            {
                speed = HiddenFloatByte.Get(value);
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

    public class TextureScroll : ScrollObject
    {
        public int segmentedAddress;

        public TextureScroll(ROM rom) : base(rom)
        {
            segmentedAddress = rom.Read32(0x10);
        }

        public TextureScroll(int segmentedAddress, byte speed, byte acts, TextureAxis axis, int behav, int offset) : base(speed, acts, axis, offset, behav)
        {
            if (speed < 16)
                throw new Exception("TextureScroll can't scroll less than 16 UVs per frame");

            this.segmentedAddress = segmentedAddress;
        }

        public short X
        {
            get
            {
                return axis == TextureAxis.X ? HiddenFloatByte.Make((byte) (-speed / 16)) : (short) 0;
            }
            set
            {
                if (value != 0)
                    speed = (byte) (-HiddenFloatByte.Get(value) * 16);
            }
        }

        public short Y
        {
            get
            {
                return axis == TextureAxis.Y ? HiddenFloatByte.Make((byte) (-speed / 16)) : (short) 0;
            }
            set
            {
                if (value != 0)
                    speed = (byte) (-HiddenFloatByte.Get(value) * 16);
            }
        }

        public override void WriteScroll(ROM rom)
        {
            rom.PushOffset(romOffset);
            rom.Write8(acts, 0x02);
            rom.Write16(X, 0x04);
            rom.Write16(Y, 0x06);
            rom.Write32(segmentedAddress, 0x10);
            rom.Write32(behavior, 0x14);
            rom.PopOffset();
        }
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

        public ScrollObject(byte speed, byte acts, TextureAxis axis, int romOffset, int behavior) : base(speed, acts, axis)
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

    public class Scroll : IEquatable<Scroll>
    {
        public byte speed;
        public byte acts;
        public TextureAxis axis;

        public Scroll(byte speed, byte acts, TextureAxis axis) 
        {
            this.speed = speed;
            this.acts = acts;
            this.axis = axis;
        }

        public Scroll() { }

        public override int GetHashCode()
        {
            return speed ^ acts ^ (int)axis;
        }

        public bool Equals(Scroll other)
        {
            if (this == null || other == null)
                return false;

            return speed == other.speed && acts == other.acts && axis == other.axis;
        }
    }
}
