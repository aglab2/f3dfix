using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelCombiner
{
    class DynamicRegion : Region
    {
        // TODO: Bullshit, make like bunch of classes or something
        public DynamicRegion(int start, int length, RegionState state) : base(start, length, state)
        {
            if (state == RegionState.LevelFooter || state == RegionState.LevelHeader || state == RegionState.ModelsLoader
             || state == RegionState.AreaFooter  || state == RegionState.AreaHeader  || state == RegionState.AreaData
             || state == RegionState.GeoLayout   || state == RegionState.DisplayList)
                throw new ArgumentException("Using DynamicRegion with given state is not allowed");
        }

        public DynamicRegion(string dirname, RegionState state, int area = -1, int model = -1) : base(dirname, state, area, model)
        {
            if (state == RegionState.LevelFooter || state == RegionState.LevelHeader || state == RegionState.ModelsLoader
             || state == RegionState.AreaFooter  || state == RegionState.AreaHeader  || state == RegionState.AreaData
             || state == RegionState.GeoLayout   || state == RegionState.DisplayList)
                throw new ArgumentException("Using DynamicRegion with given state is not allowed");
        }

        public override void Relocate(RelocationTable table = null) { }
    }
}
