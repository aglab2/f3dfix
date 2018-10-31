using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelCombiner
{
    class GeoLayoutRegion : Region
    {
        public GeoLayoutRegion(int start, int length) : base(start, length, RegionState.GeoLayout) { }

        public GeoLayoutRegion(string dirname, int area = -1, int model = -1) : base(dirname, RegionState.GeoLayout, area, model) { }

        public override void Relocate(RelocationTable table)
        {
            GeoLayout.PerformRegionRelocation(this, table);
        }
    }
}
