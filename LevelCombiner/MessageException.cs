using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelCombiner
{
    class MessageException : Exception
    {
        public MessageException(string name) : base(name) { }
    }
}
