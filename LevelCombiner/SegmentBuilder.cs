using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelCombiner
{
    class DataBuilder
    {
        // Offset is aligned on 0x10
        int start;
        int offset;
        byte[] data;

        int backedUpOffset;

        public int Offset { get { return offset; } set { offset = value; } }
        public byte[] Data { get { return data; } }

        public DataBuilder(int start, int length)
        {
            data = new byte[length];
            this.start = start;
            offset = 0;
        }

        public void RoundOffset()
        {
            double notRoundOffset = offset;
            offset = ((int) Math.Ceiling(notRoundOffset / 0x10) * 0x10);
        }

        public void AddRegion(Region region)
        {
            byte[] newData = region.data;

            int length = newData.Length;
            if (data.Length < offset + length)
            {
                throw new OutOfMemoryException("No more data in segment available");
            }

            Array.Copy(newData, 0, data, offset, length);
            region.romStart = start + offset;
            offset += length;
        }

        public void Backup()
        {
            backedUpOffset = offset;
        }

        public void Restore()
        {
            offset = backedUpOffset;
        }
    }
}
