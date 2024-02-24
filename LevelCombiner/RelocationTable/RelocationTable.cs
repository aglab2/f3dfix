using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelCombiner
{
    public class RelocationUnit
    {
        int start;
        int length;
        int newStart;

        public RelocationUnit(int start, int length, int newStart)
        {
            this.start    = start;
            this.length   = length;
            this.newStart = newStart;
        }

        // Generates static relocation unit from or to static
        public RelocationUnit(Region region, ROM rom, bool isFromStatic)
        {
            int segmentedAddress = rom.GetSegmentedAddress(region.romStart);
            if (segmentedAddress == -1)
                throw new ArgumentException("Failed to get segmented address");

            int segment = SegmentedAddressHelper.GetSegment(segmentedAddress);
            int staticSegmentedAddress = SegmentedAddressHelper.GetStaticSegmentedAddress(segment);

            length = region.maxLength;

            if (!isFromStatic)
            {
                start = segmentedAddress;
                newStart = staticSegmentedAddress;
            }
            else
            {
                start = staticSegmentedAddress;
                newStart = segmentedAddress;
            }
        }

        public int Relocate(int oldAddress)
        {
            if (start <= oldAddress && oldAddress < start + length)
            {
                int diff = oldAddress - start;
                return diff + newStart;
            }

            return -1;
        }
    }

    abstract public class RelocationTable
    {
        public void AddUnit(RelocationUnit unit) { AddUnit(null, unit); }
        public abstract void AddUnit(object key, RelocationUnit unit);
        public abstract int Relocate(object key, int address);

        public void RelocateOffset(ROM rom, int offset) { RelocateOffset(null, rom, offset); }
        public void RelocateOffset(object key, ROM rom, int romOffset)
        {
            int segment = rom.Read8(romOffset);
            if (segment != 0x0e && segment != 0x19)
                return;

            int segmentedAddress = rom.Read32(romOffset);

            int newSegmentedAddress = Relocate(key, segmentedAddress);
            if (newSegmentedAddress == -1)
                throw new ArgumentException(String.Format("Relocation Table does not have address {0:x}", segmentedAddress));

            rom.Write32(newSegmentedAddress, romOffset);
        }
    }
}
