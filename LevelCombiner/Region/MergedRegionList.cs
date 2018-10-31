using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LevelCombiner
{
    class MergedRegionList
    {
        public int start;
        public int length;

        public MergedRegionList()
        {
            start = 0;
            length = 0;
        }
        
        public void AddRegion(int start, int length)
        {
            if (this.start == 0 && this.length == 0)
            {
                this.start  = start;
                this.length = length;
                return;
            }

            if (this.start > start)
                this.start = start;

            if (this.start + this.length < start + length)
                this.length = start + length - this.start;
        }
    }
}
