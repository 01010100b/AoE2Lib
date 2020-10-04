using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AoE2Lib
{
    class AoCInstance : AoE2Instance
    {
        public AoCInstance(Process process) : base(process) { }

        public override int[] GetGoals(int player)
        {
            throw new NotImplementedException();
        }

        public override int[] GetStrategicNumbers(int player)
        {
            throw new NotImplementedException();
        }

        public override bool SetGoal(int player, int index, int value)
        {
            throw new NotImplementedException();
        }

        public override bool SetGoals(int player, int start_index, params int[] values)
        {
            throw new NotImplementedException();
        }

        public override bool SetStrategicNumber(int player, int index, int value)
        {
            throw new NotImplementedException();
        }

        public override bool SetStrategicNumbers(int player, int start_index, params int[] values)
        {
            throw new NotImplementedException();
        }
    }
}
