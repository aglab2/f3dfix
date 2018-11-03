using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LevelCombiner
{
    public class SegmentDescriptor
    {
        public int start;
        public int length;

        public SegmentDescriptor(int start, int length)
        {
            this.start = start;
            this.length = length;
        }
    }

    public class ROM : PositionalBuffer, ICloneable
    {
        public SegmentDescriptor[] segments;
        public Stack<int> offsetStack;

        public ROM(byte[] rom) : base(rom)
        {
            segments = new SegmentDescriptor[0x20];
            for (int i = 0; i < 0x20; i++)
                segments[i] = null;

            offsetStack = new Stack<int>();
        }

        public void SetSegment(int segment, SegmentDescriptor descriptor)
        {
            if (segment > 0x20)
                return;

            segments[segment] = descriptor;
        }

        public int GetROMAddress(int segmentedAddress)
        {
            int segment = SegmentedAddressHelper.GetSegment(segmentedAddress);
            int offset  = SegmentedAddressHelper.GetOffset(segmentedAddress); 

            if (segment > 0x20)
                return -1;

            if (segments[segment] == null)
                return -1;

            return segments[segment].start + offset;
        }

        public int GetSegmentedAddress(int romAddress)
        {
            for (int currentSegment = 0; currentSegment < 0x20; currentSegment++)
            {
                SegmentDescriptor descriptor = segments[currentSegment];
                if (descriptor == null)
                    continue;

                if (descriptor.start <= romAddress && romAddress <= descriptor.start + descriptor.length)
                    return SegmentedAddressHelper.GetSegmentedAddress(currentSegment, romAddress - descriptor.start);
            }

            return -1;
        }

        public void PushOffset(int newOffset)
        {
            offsetStack.Push(offset);
            offset = newOffset;
        }

        public void PopOffset()
        {
            offset = offsetStack.Pop();
        }

        public void ReadData(int offset, int length, byte[] source)
        {
            Array.Copy(rom, offset, source, 0, length);
        }

        public SegmentDescriptor GetSegmentDescriptor(int segment)
        {
            if (segment >= 0x20)
                return null;

            return segments[segment];
        }

        public object Clone()
        {
            ROM retRom = new ROM((byte[])rom.Clone())
            {
                segments = (SegmentDescriptor[])segments.Clone(),
                offset = offset
            };
            return retRom;
        }
    }
}
