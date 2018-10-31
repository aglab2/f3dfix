using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelCombiner
{
    class LevelScriptRegion : Region
    {
        public LevelScriptRegion(int start, int length, RegionState state) : base(start, length, state)
        {
            if (state != RegionState.LevelFooter && state != RegionState.LevelHeader && state != RegionState.ModelsLoader
             && state != RegionState.AreaFooter  && state != RegionState.AreaHeader  && state != RegionState.AreaData
             && state != RegionState.AreaScrolls)
                throw new Exception("Level Script Region was used with bad state");
        }

        public LevelScriptRegion(string dirname, RegionState state, int area = -1, int model = -1) : base(dirname, state, area, model)
        {
            if (state != RegionState.LevelFooter && state != RegionState.LevelHeader && state != RegionState.ModelsLoader
             && state != RegionState.AreaFooter  && state != RegionState.AreaHeader  && state != RegionState.AreaData
             && state != RegionState.AreaScrolls)
                throw new Exception("Level Script Region was used with bad state");
        }

        public override void Relocate(RelocationTable table)
        {
            LevelScript.PerformRegionRelocation(this, table);
        }
    }
}
