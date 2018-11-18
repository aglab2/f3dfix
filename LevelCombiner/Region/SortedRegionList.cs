using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelCombiner
{
    class SortedRegionList
    {
        private SortedList<int, int> regionList;

        public SortedRegionList()
        {
            regionList = new SortedList<int, int>();
        }

        public SortedList<int, int> RegionList { get { return regionList; } }

        public void AddRegion(int start, int length)
        {
            if (start == 0)
                throw new ArgumentNullException("ROM Region Start is 0, bug?");

            if (length == 0)
                return;

            regionList.TryGetValue(start, out int prevLength);

            // Key was already there, update value
            if (prevLength != 0)
            {
                if (length > prevLength)
                {
                    regionList.Remove(start);
                }
                else
                {
                    // Length is shorter, just go away
                    return;
                }
            }

            regionList.Add(start, length);
            Merge(start);
        }

        void Merge(int key)
        {
            int index = -1;
            do
            {
                index = regionList.IndexOfKey(key);
                if (index == regionList.Count - 1)
                    break;
            }
            while (MergeBackwards(index + 1));

            index = regionList.IndexOfKey(key);
            while (MergeBackwards(index))
            {
                index--;
            };
        }

        bool MergeBackwards(int index)
        {
            if (index == 0)
                return false;

            KeyValuePair<int, int> cur = regionList.ElementAt(index);
            KeyValuePair<int, int> prev = regionList.ElementAt(index - 1);
            if (prev.Key + prev.Value < cur.Key)
                return false;

            regionList.Remove(prev.Key);
            regionList.Remove(cur.Key);
            regionList.Add(prev.Key, Math.Max(prev.Value, (int) (cur.Value + (cur.Key - prev.Key))));

            return true;
        }

        public void CutContigRegion(int size, int round, out int vertexStart, out int vertexLength, out bool isRegionTrimmed)
        {
            // Cut a bit from available space for vertices
            KeyValuePair<int, int> region = RegionList.First();
            RegionList.RemoveAt(0);
            vertexStart = region.Key;
            vertexLength = region.Value;

            isRegionTrimmed = vertexLength > size;
            // vertex buffer is size max size
            if (isRegionTrimmed)
            {
                // Trim and put back data if left
                RegionList.Add(vertexStart + size, vertexLength - size);
                vertexLength = size;
            }
            else
            {
                // Round to closest
                vertexLength = vertexLength / round * round;
            }    
        }
    }
}
