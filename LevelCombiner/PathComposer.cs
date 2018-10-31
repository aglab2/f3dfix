using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelCombiner
{
    class PathComposer
    {
        public static string ComposeName(string dirname, RegionState state, int area = -1, int model = -1, int number = -1)
        {
            string regionPath = null;
            switch (state)
            {
                case RegionState.LevelHeader:
                    regionPath = Path.Combine(dirname, "header");
                    break;
                case RegionState.LevelFooter:
                    regionPath = Path.Combine(dirname, "footer");
                    break;
                case RegionState.ModelsLoader:
                    regionPath = Path.Combine(dirname, "models", "loader");
                    break;

                case RegionState.AreaHeader:
                    if (area == -1)
                        throw new ArgumentException("Orphan AreaHeader!");
                    regionPath = Path.Combine(dirname, "area" + area, "header");
                    break;
                case RegionState.AreaData:
                    if (area == -1)
                        throw new ArgumentException("Orphan AreaData!");
                    regionPath = Path.Combine(dirname, "area" + area, "objects");
                    break;
                case RegionState.AreaScrolls:
                    if (area == -1)
                        throw new ArgumentException("Orphan AreaScrolls!");
                    regionPath = Path.Combine(dirname, "area" + area, "scrolls");
                    break;

                case RegionState.AreaFooter:
                    if (area == -1)
                        throw new ArgumentException("Orphan AreaFooter!");
                    regionPath = Path.Combine(dirname, "area" + area, "footer");
                    break;

                case RegionState.GeoLayout:
                    if (area != -1)
                        regionPath = Path.Combine(dirname, "area" + area, "geolayout");
                    else if (model != -1)
                        regionPath = Path.Combine(dirname, "models", "geolayout" + model);
                    else
                        throw new ArgumentException("Orphan Geolayout!");

                    break;

                case RegionState.Collision:
                    if (area == -1)
                        throw new ArgumentException("Orphan Collision!");
                    regionPath = Path.Combine(dirname, "area" + area, "collision");
                    break;
                case RegionState.DisplayList:
                    if (area != -1)
                        regionPath = Path.Combine(dirname, "area" + area, "graphics", "disp" + number);
                    else if (model != -1)
                        regionPath = Path.Combine(dirname, "models", "graphics" + model, "disp" + number);
                    else
                        throw new ArgumentException("Orphan DisplayList!");
                    break;

                case RegionState.GraphicsData:
                    if (area != -1)
                        regionPath = Path.Combine(dirname, "area" + area, "graphics", "data");
                    else if (model != -1)
                        regionPath = Path.Combine(dirname, "models", "graphics" + model, "data");
                    else
                        throw new ArgumentException("Orphan GraphicsData!");
                    break;
            }

            return regionPath;
        }
        
        public static bool IsRegionFileExists(string dirname, RegionState state, int area = -1, int model = -1, int number = -1)
        {
            string path = ComposeName(dirname, state, area, model, number);
            return File.Exists(path);
        }
    }
}
