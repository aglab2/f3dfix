using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelCombiner
{
    class KeyRelocationTable : RelocationTable
    {
        Dictionary<int, RelocationUnit> table;

        public KeyRelocationTable()
        {
            table = new Dictionary<int, RelocationUnit>();
        }

        public override void AddUnit(object keyObj, RelocationUnit unit)
        {
            int key = (int)keyObj;
            table.Add(key, unit);
        }

        public override int Relocate(object keyObj, int address)
        {
            int key = (int)keyObj;
            return table[key].Relocate(address);
        }
    }
}
