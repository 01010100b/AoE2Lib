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
            ALL, MILITARY, CIVILIAN, RESOURCE
        }

        internal readonly List<Position> TilesToCheck = new List<Position>();
        internal int UnitSearch1Player { get; private set; } = -1;
        internal Position UnitSearch1Position { get; private set; } = new Position(-1, -1);
        internal int UnitSearch1Radius { get; private set; } = -1;
        internal UnitSearchType UnitSearch1Type { get; private set; } = UnitSearchType.ALL;
        internal int UnitSearch2Player { get; private set; } = -1;
        internal Position UnitSearch2Position { get; private set; } = new Position(-1, -1);
        internal int UnitSearch2Radius { get; private set; } = -1;
        internal UnitSearchType UnitSearch2Type { get; private set; } = UnitSearchType.ALL;
        internal int UnitTypeInfoPlayer { get; private set; } = -1;
        internal int UnitTypeInfoType { get; private set; } = -1;

        public void CheckTile(Position position)
        {
            TilesToCheck.Add(position);
        }

        public void SearchForUnits1(int player, Position position, int radius, UnitSearchType type)
        {
            UnitSearch1Player = player;
            UnitSearch1Position = position;
            UnitSearch1Radius = radius;
            UnitSearch1Type = type;
        }

        public void SearchForUnits2(int player, Position position, int radius, UnitSearchType type)
        {
            UnitSearch2Player = player;
            UnitSearch2Position = position;
            UnitSearch2Radius = radius;
            UnitSearch2Type = type;
        }

        public void GetUnitTypeInfo(int player, int type)
        {
            UnitTypeInfoPlayer = player;
            UnitTypeInfoType = type;
        }
    }
}
