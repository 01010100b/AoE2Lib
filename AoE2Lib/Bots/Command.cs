using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using static AoE2Lib.Bots.Command.UnitSearchCommand;

namespace AoE2Lib.Bots
{
    public class Command
    {
        public struct UnitSearchCommand
        {
            public enum UnitSearchType
            {
                MILITARY, CIVILIAN, BUILDING, WOOD, FOOD, GOLD, STONE
            }

            public readonly int Player;
            public readonly Position Position;
            public readonly int Radius;
            public readonly UnitSearchType SearchType;

            public UnitSearchCommand(int player, Position position, int radius, UnitSearchType type)
            {
                Player = player;
                Position = position;
                Radius = radius;
                SearchType = type;
            }
        }

        internal readonly List<Position> TilesToCheck = new List<Position>();
        internal readonly List<UnitSearchCommand> UnitSearchCommands = new List<UnitSearchCommand>();
        internal int UnitTypeInfoPlayer { get; private set; } = -1;
        internal int UnitTypeInfoType { get; private set; } = -1;
        internal readonly List<int> Training = new List<int>();
        internal readonly List<int> Building = new List<int>();

        public void CheckTile(Position position)
        {
            TilesToCheck.Add(position);
        }

        public void SearchForUnits(UnitSearchCommand search)
        {
            UnitSearchCommands.Add(search);
        }

        public void CheckUnitTypeInfo(int player, int type)
        {
            UnitTypeInfoPlayer = player;
            UnitTypeInfoType = type;
        }

        public void Train(int unit, int max_pending = 0)
        {
            var goal = Math.Max(0, Math.Min(unit, 1999));
            goal *= 100;
            goal += Math.Max(-1, Math.Min(max_pending, 98)) + 1;

            Training.Add(goal);
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

            Building.Add(goal);
        }
    }
}
