using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using static AoE2Lib.Bots.UnitTypeInfo;

namespace AoE2Lib.Bots
{
    public class GameState
    {
        public Position MyPosition { get; private set; } = new Position(-1, -1);
        public int MapWidthHeight { get; private set; } = -1;
        public IReadOnlyList<Player> Players => _Players;
        private readonly List<Player> _Players = new List<Player>();
        public IReadOnlyDictionary<Position, Tile> Tiles => _Tiles;
        private readonly Dictionary<Position, Tile> _Tiles = new Dictionary<Position, Tile>();
        public IReadOnlyDictionary<int, Unit> Units => _Units;
        private readonly Dictionary<int, Unit> _Units = new Dictionary<int, Unit>();
        public IReadOnlyDictionary<UnitTypeKey, UnitTypeInfo> UnitTypeInfos => _UnitTypeInfos;
        private readonly Dictionary<UnitTypeKey, UnitTypeInfo> _UnitTypeInfos = new Dictionary<UnitTypeKey, UnitTypeInfo>();

        internal void Update(int[] goals, int[] sns)
        {

        }
    }
}
