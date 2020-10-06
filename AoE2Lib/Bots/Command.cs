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

        private readonly List<Position> TilesToCheck = new List<Position>();
        private int UnitSearch1Player { get; set; } = -1;
        private Position UnitSearch1Position { get; set; } = new Position(-1, -1);
        private int UnitSearch1Radius { get; set; } = -1;
        private UnitSearchType UnitSearch1Type { get; set; } = UnitSearchType.ALL;
        private int UnitSearch2Player { get; set; } = -1;
        private Position UnitSearch2Position { get; set; } = new Position(-1, -1);
        private int UnitSearch2Radius { get; set; } = -1;
        private UnitSearchType UnitSearch2Type { get; set; } = UnitSearchType.ALL;
        private int UnitTypeInfoPlayer { get; set; } = -1;
        private int UnitTypeInfoType { get; set; } = -1;

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
    }
}
