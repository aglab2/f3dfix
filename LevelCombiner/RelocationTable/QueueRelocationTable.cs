using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelCombiner
{
    class QueueRelocationTable : RelocationTable
    {
        Queue<RelocationUnit> table;

        public QueueRelocationTable()
        {
            table = new Queue<RelocationUnit>();
        }

        public override void AddUnit(object key, RelocationUnit unit)
        {
            table.Enqueue(unit);
        }

        public override int Relocate(object key, int address)
        {
            RelocationUnit unit = table.Peek();
            int newAddress = unit.Relocate(address);

            if (newAddress == -1)
                return -1;

            table.Dequeue();
            return newAddress;
        }
    }
}
