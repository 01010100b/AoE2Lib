using AoE2Lib.Bots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaternary
{
    abstract class Strategy
    {
        public readonly Dictionary<StrategicNumber, int> StrategicNumbers = new Dictionary<StrategicNumber, int>();

        public Command GetNextCommand(Quaternary quaternary, GameState state)
        {
            return GetCommand(quaternary, state);
        }

        protected abstract Command GetCommand(Quaternary quaternary, GameState state);
    }
}
