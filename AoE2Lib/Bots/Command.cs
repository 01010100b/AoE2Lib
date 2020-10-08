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
            MILITARY, CIVILIAN, BUILDING, WOOD, FOOD, GOLD, STONE
        }

        internal struct UnitSearchCommand
        {
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

        public void CheckTile(Position position)
        {
            TilesToCheck.Add(position);
        }

        public void SearchForUnits(int player, Position position, int radius, UnitSearchType type)
        {
            UnitSearchCommands.Add(new UnitSearchCommand(player, position, radius, type));
        }

        public void GetUnitTypeInfo(int player, int type)
        {
            UnitTypeInfoPlayer = player;
            UnitTypeInfoType = type;
        }
    }
}
