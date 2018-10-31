using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelCombiner
{
    class StaticRelocationTable : RelocationTable
    {
        List<RelocationUnit> table;

        public StaticRelocationTable()
        {
            table = new List<RelocationUnit>();
        }
        
        public override void AddUnit(object key, RelocationUnit unit)
        {
            table.Add(unit);
        }

        public override int Relocate(object key, int address)
        {
            foreach (RelocationUnit unit in table)
            {
                int newAddress = unit.Relocate(address);
                if (newAddress != -1)
                    return newAddress;
            }

            return -1;
        }
    }
}
