using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using static AoE2Lib.Bots.UnitTypeInfo;

namespace AoE2Lib.Bots
{
    public class GameState
    {
        public TimeSpan GameTime { get; internal set; } = TimeSpan.Zero;
        public int WoodAmount { get; internal set; } = -1;
        public int FoodAmount { get; internal set; } = -1;
        public int GoldAmount { get; internal set; } = -1;
        public int StoneAmount { get; internal set; } = -1;
        public int PopulationHeadroom { get; internal set; } = -1;
        public int HousingHeadroom { get; internal set; } = -1;
        public Position MyPosition { get; internal set; } = new Position(-1, -1);
        public int MapWidthHeight { get; internal set; } = -1;
        public IReadOnlyList<Player> Players => _Players;
        internal readonly List<Player> _Players = new List<Player>();
        public IReadOnlyDictionary<Position, Tile> Tiles => _Tiles;
        internal readonly Dictionary<Position, Tile> _Tiles = new Dictionary<Position, Tile>();
        public IReadOnlyDictionary<int, Unit> Units => _Units;
        internal readonly Dictionary<int, Unit> _Units = new Dictionary<int, Unit>();
        public IReadOnlyDictionary<UnitTypeKey, UnitTypeInfo> UnitTypeInfos => _UnitTypeInfos;
        internal readonly Dictionary<UnitTypeKey, UnitTypeInfo> _UnitTypeInfos = new Dictionary<UnitTypeKey, UnitTypeInfo>();

        public override string ToString()
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("--- CURRENT STATE ---");
            sb.AppendLine();

            sb.AppendLine($"game time: {GameTime}");
            sb.AppendLine($"Wod {WoodAmount} Food {FoodAmount} Gold {GoldAmount} Stone {StoneAmount} Population Headroom {PopulationHeadroom} Housing Headroom {HousingHeadroom}");

            return sb.ToString();
        }
    }
}
