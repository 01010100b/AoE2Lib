using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots
{
    public class Command
    {
        public enum UnitSearchType
        {
            MILITARY, CIVILIAN, BUILDING, WOOD, FOOD, GOLD, STONE, ALL
        }

        internal struct DoubleCommand
        {
            public readonly int Sn0;
            public readonly int Sn1;

            public DoubleCommand(int sn0, int sn1)
            {
                Sn0 = sn0;
                Sn1 = sn1;
            }
        }

        internal readonly List<int> UnitSearchCommands = new List<int>();
        internal readonly List<int> CheckTileCommands = new List<int>();
        internal readonly List<int> UnitTypeInfoCommands = new List<int>();
        internal readonly List<int> TrainCommands = new List<int>();
        internal readonly List<int> BuildCommands = new List<int>();

        public void CheckTile(Position position)
        {
            var x = Math.Max(0, Math.Min(499, position.X));
            var y = Math.Max(0, Math.Min(499, position.Y));

            var sn = x;
            sn *= 500;
            sn += y;

            CheckTileCommands.Add(sn);
        }

        public void SearchForUnits(int player, Position position, int radius, UnitSearchType type)
        {
            player = Math.Max(0, Math.Min(8, player));
            var x = Math.Max(0, Math.Min(499, position.X));
            var y = Math.Max(0, Math.Min(499, position.Y));
            position = new Position(x, y);
            radius = Math.Max(0, Math.Min(99, radius));

            var sn = player;
            sn *= 500;
            sn += position.X;
            sn *= 500;
            sn += position.Y;
            sn *= 100;
            sn += radius;
            sn *= 8;
            sn += (int)type;

            UnitSearchCommands.Add(sn);
        }

        public void CheckUnitTypeInfo(int player, int type)
        {
            player = Math.Max(0, Math.Min(8, player));
            type = Math.Max(0, Math.Min(1999, type));

            var sn = player;
            sn *= 2000;
            sn += type;

            UnitTypeInfoCommands.Add(sn);
        }

        public void Train(int unit, int max_pending = 0)
        {
            var goal = Math.Max(0, Math.Min(unit, 1999));
            goal *= 100;
            goal += Math.Max(-1, Math.Min(max_pending, 98)) + 1;

            TrainCommands.Add(goal);
        }

        public void Build(int building, Position position, int max_pending = 0)
        {
            var goal = Math.Max(0, Math.Min(building, 1999));
            goal *= 500;
            goal += Math.Max(0, Math.Min(position.X, 499));
            goal *= 500;
            goal += Math.Max(0, Math.Min(position.Y, 499));
            goal *= 4;
            goal += Math.Max(-1, Math.Min(max_pending, 2)) + 1;

            BuildCommands.Add(goal);
        }
    }
}
