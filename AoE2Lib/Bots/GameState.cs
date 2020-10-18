using AoE2Lib.Bots.GameElements;
using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static AoE2Lib.Bots.GameElements.UnitTypeInfo;

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

        internal void Update(int[] goals)
        {
            UpdateInfo(goals);
            UpdatePlayers(goals);
            UpdateTiles(goals);
            UpdateUnits(goals);
            UpdateUnitsTargetable(goals);
        }

        private void UpdateInfo(int[] goals)
        {
            const int GL_GAMETIME = 11 - 1;
            const int GL_MAPSIZE = 12 - 1;

            const int GL_WOOD = 21 - 1;
            const int GL_FOOD = 22 - 1;
            const int GL_GOLD = 23 - 1;
            const int GL_STONE = 24 - 1;
            const int GL_POPULATION_HEADROOM = 25 - 1;
            const int GL_HOUSING_HEADROOM = 26 - 1;
            const int GL_X = 27 - 1;
            const int GL_Y = 28 - 1;

            GameTime = TimeSpan.FromSeconds(goals[GL_GAMETIME]);
            MapWidthHeight = goals[GL_MAPSIZE];

            WoodAmount = goals[GL_WOOD];
            FoodAmount = goals[GL_FOOD];
            GoldAmount = goals[GL_GOLD];
            StoneAmount = goals[GL_STONE];
            PopulationHeadroom = goals[GL_POPULATION_HEADROOM];
            HousingHeadroom = goals[GL_HOUSING_HEADROOM];
            MyPosition = new Position(goals[GL_X], goals[GL_Y]);
        }

        private void UpdatePlayers(int[] goals)
        {
            const int GL_PLAYER_GOAL0 = 41 - 1;
            const int GL_PLAYER_GOAL1 = 42 - 1;

            var goal0 = goals[GL_PLAYER_GOAL0];
            var goal1 = goals[GL_PLAYER_GOAL1];

            if (goal0 >= 0)
            {
                var player = goal0 % 10;

                if (!Players.ContainsKey(player))
                {
                    _Players.Add(player, new Player(player));
                }

                Players[player].Update(goal0, goal1);
            }

            if (!Players.ContainsKey(0))
            {
                _Players.Add(0, new Player(0));
            }
        }

        private void UpdateTiles(int[] goals)
        {
            const int GL_TILES_START = 51 - 1;
            const int GL_TILES_END = 90 - 1;

            var offset = GL_TILES_START;
            while (offset <= GL_TILES_END)
            {
                var goal0 = goals[offset];
                offset++;
                var goal1 = goals[offset];
                offset++;

                if (goal0 >= 0)
                {
                    var x = goal0 / 500;
                    var y = goal0 % 500;
                    var position = new Position(x, y);

                    Tiles[position].Update(goal0, goal1);
                }
            }
        }

        private void UpdateUnits(int[] goals)
        {
            const int GL_UNITS_START = 151 - 1;
            const int GL_UNITS_END = 390 - 1;

            var offset = GL_UNITS_START;
            while (offset <= GL_UNITS_END)
            {
                var goal0 = goals[offset];
                offset++;
                var goal1 = goals[offset];
                offset++;
                var goal2 = goals[offset];
                offset++;

                if (goal0 >= 0)
                {
                    var id = goal0 % 45000;

                    if (!Units.ContainsKey(id))
                    {
                        _Units.Add(id, new Unit(id));
                    }

                    var unit = Units[id];
                    if (Tiles.TryGetValue(unit.Position, out Tile tile))
                    {
                        tile._Units.Remove(unit);
                    }

                    unit.Update(goal0, goal1, goal2);
                    if (Tiles.TryGetValue(unit.Position, out tile))
                    {
                        tile._Units.Add(unit);
                    }

                    var key = new UnitTypeInfoKey(unit.PlayerNumber, unit.TypeId);
                    if (!UnitTypeInfos.ContainsKey(key))
                    {
                        _UnitTypeInfos.Add(key, new UnitTypeInfo(key));
                    }
                }
            }
        }

        private void UpdateUnitsTargetable(int[] goals)
        {
            const int GL_UNITTARGETABLE_START = 34 - 1;
            const int GL_UNITTARGETABLE_END = 35 - 1;

            var offset = GL_UNITTARGETABLE_START;
            while (offset <= GL_UNITTARGETABLE_END)
            {
                var goal0 = goals[offset];
                offset++;

                if (goal0 >= 0)
                {
                    var unit = goal0 / 2;
                    var targetable = goal0 % 2 == 1;

                    if (Units.TryGetValue(unit, out Unit u))
                    {
                        u.UpdateTargetable(targetable);
                    }
                }
            }
        }
    }
}
