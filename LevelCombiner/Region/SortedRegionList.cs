using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelCombiner
{
    public class SortedRegionList : IEquatable<SortedRegionList>
    {
        private SortedList<int, int> regionList;

        public SortedRegionList()
        {
            regionList = new SortedList<int, int>();
        }

        public bool Equals(SortedRegionList other)
        {
            return Enumerable.SequenceEqual(regionList, other.RegionList);
        }

        public SortedList<int, int> RegionList { get { return regionList; } }

        public void AddRegion(int start, int length)
        {
            if (start == 0)
                throw new ArgumentNullException("ROM Region Start is 0, bug?");

            if (length == 0 || length < 0)
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

        public void AddRegions(SortedRegionList list)
        {
            foreach (KeyValuePair<int, int> kvp in list.regionList)
            {
                AddRegion(kvp.Key, kvp.Value);
            }
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

        public void CutContigRegion(int size, out int vertexStart)
        {
            var fittingRegions = RegionList.Where(kv => kv.Value >= size).ToList();
            if (fittingRegions.Count() == 0)
                throw new Exception("No contig region can be found!");

            KeyValuePair<int, int> mostFittingRegion = fittingRegions.First();
            foreach (KeyValuePair<int, int> region in RegionList)
            {
                if (region.Value < size)
                    continue;

                if (region.Value < mostFittingRegion.Value)
                    mostFittingRegion = region;
            }

            RegionList.Remove(mostFittingRegion.Key);
            RegionList.Add(mostFittingRegion.Key + size, mostFittingRegion.Value - size);
            vertexStart = mostFittingRegion.Key;
        }
    }
}
