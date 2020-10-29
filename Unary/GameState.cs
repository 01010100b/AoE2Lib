using Unary.GameElements;
using Unary.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Unary.GameElements.UnitTypeInfo;

namespace Unary
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
        public int MapWidthHeight
        {
            get
            {
                return _MapWidthHeight;
            }
            set
            {
                _MapWidthHeight = value;
                
                var size = value * value;

                if (_Tiles.Count != size)
                {
                    _Tiles.Clear();

                    for (int x = 0; x < value; x++)
                    {
                        for (int y = 0; y < value; y++)
                        {
                            var tile = new Tile(new Position(x, y));
                            _Tiles.Add(tile.Position, tile);
                        }
                    }
                }
            }
        }
        private int _MapWidthHeight { get; set; } = -1;
        public IReadOnlyDictionary<int, Player> Players => _Players;
        internal readonly Dictionary<int, Player> _Players = new Dictionary<int, Player>();
        public IReadOnlyDictionary<Position, Tile> Tiles => _Tiles;
        internal readonly Dictionary<Position, Tile> _Tiles = new Dictionary<Position, Tile>();
        public IReadOnlyDictionary<int, Unit> Units => _Units;
        internal readonly Dictionary<int, Unit> _Units = new Dictionary<int, Unit>();
        public IReadOnlyDictionary<UnitTypeInfoKey, UnitTypeInfo> UnitTypeInfos => _UnitTypeInfos;
        internal readonly Dictionary<UnitTypeInfoKey, UnitTypeInfo> _UnitTypeInfos = new Dictionary<UnitTypeInfoKey, UnitTypeInfo>();
    }
}
