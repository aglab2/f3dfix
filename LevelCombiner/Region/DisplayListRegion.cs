using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelCombiner
{
    public class DisplayListRegion : Region
    {
        public bool DLFixesNeeded;
        public bool isFogEnabled;
        public bool isEnvcolorEnabled;
        public Int64 FCcmdfirst = 0;
        public int layer = 4;
        public bool isUnusedTrimmingAllowed;

        public DisplayListRegion(int start, int length, bool isFogEnabled, bool isEnvcolorEnabled, Int64 FCcmd, int layer, bool isUnusedTrimmingAllowed) : base(start, length, RegionState.DisplayList)
        {
            DLFixesNeeded = true;
            this.isFogEnabled =      isFogEnabled;
            this.isEnvcolorEnabled = isEnvcolorEnabled;
            this.FCcmdfirst = FCcmd;
            this.layer = layer;
            this.isUnusedTrimmingAllowed = isUnusedTrimmingAllowed;
        }

        public DisplayListRegion(string dirname, int area = -1, int model = -1, int number = -1) : base(dirname, RegionState.DisplayList, area, model, number)
        {
            DLFixesNeeded = false;
        }

        public override void Relocate(RelocationTable table)
        {
        }
    }
}
