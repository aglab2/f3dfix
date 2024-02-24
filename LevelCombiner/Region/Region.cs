using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelCombiner
{
    public enum RegionState
    {
        LevelHeader,
        ModelsLoader,
        AreaHeader,
        AreaData,
        AreaScrolls,
        AreaFooter,
        LevelFooter,
        GeoLayout,
        Collision,
        TextureInfo,
        VertexInfo,
        LightData,
        DisplayList,
        GraphicsData
    }

    abstract public class Region
    {

        public int romStart;
        public int maxLength;
        public RegionState state;
        public byte[] data;

        public int area = -1;
        public int model = -1;
        public int number = -1;
        
        // Empty region init
        public Region(int start, int maxLength, RegionState state)
        {
            this.romStart = start;
            this.maxLength = maxLength;
            this.state = state;

            if (start == 0)
                throw new ArgumentNullException("ROM Region Start is 0, bug?");
        }

        // Read from dirname init
        public Region(string dirname, RegionState state, int area = -1, int model = -1, int number = -1)
        {
            string levelHeaderPath = PathComposer.ComposeName(dirname, state, area, model, number);
            this.data = File.ReadAllBytes(levelHeaderPath);
            this.maxLength = data.Length;

            this.area = area;
            this.model = model;
            this.number = number;
        }

        public abstract void Relocate(RelocationTable table);
    }
}
