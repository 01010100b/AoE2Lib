﻿using Unary.GameElements;
using Unary.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Unary.GameElements.UnitTypeInfo;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Protos.Expert.Fact;
using Protos.Expert.Action;
using System.Diagnostics;

namespace Unary
{
    public class GameState
    {
        public TimeSpan GameTime { get; private set; } = TimeSpan.Zero;
        public Position MyPosition { get; private set; } = new Position(-1, -1);
        public int MapWidthHeight
        {
            get
            {
                return _MapWidthHeight;
            }
            private set
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
        private readonly Dictionary<int, Player> _Players = new Dictionary<int, Player>();
        public IReadOnlyDictionary<Position, Tile> Tiles => _Tiles;
        private readonly Dictionary<Position, Tile> _Tiles = new Dictionary<Position, Tile>();
        public IReadOnlyDictionary<int, Unit> Units => _Units;
        private readonly Dictionary<int, Unit> _Units = new Dictionary<int, Unit>();
        public IReadOnlyDictionary<UnitTypeInfoKey, UnitTypeInfo> UnitTypeInfos => _UnitTypeInfos;
        private readonly Dictionary<UnitTypeInfoKey, UnitTypeInfo> _UnitTypeInfos = new Dictionary<UnitTypeInfoKey, UnitTypeInfo>();

        private readonly Command Command = new Command();
        private readonly Random RNG = new Random(Guid.NewGuid().GetHashCode() ^ DateTime.UtcNow.Ticks.GetHashCode());

        public void AddUnit(int id)
        {
            if (!_Units.ContainsKey(id))
            {
                _Units.Add(id, new Unit(id));
            }
        }

        internal IEnumerable<Command> RequestUpdate()
        {
            const int TILES_PER_COMMAND = 50;
            const int UNITS_PER_COMMAND = 20;
            const int UNITTYPEINFOS_PER_COMMAND = 10;

            Command.Messages.Clear();
            Command.Responses.Clear();

            // general info

            Command.Messages.Add(new GameTime());
            Command.Messages.Add(new UpGetPoint() { PositionType = 9, GoalPoint = 100 });
            Command.Messages.Add(new Goal() { GoalId = 100 });
            Command.Messages.Add(new Goal() { GoalId = 101 });
            Command.Messages.Add(new SetGoal() { GoalId = 50, GoalValue = 1000 });
            Command.Messages.Add(new SetGoal() { GoalId = 51, GoalValue = 1000 });
            Command.Messages.Add(new UpBoundPoint() { GoalPoint1 = 100, GoalPoint2 = 50 });
            Command.Messages.Add(new Goal() { GoalId = 100 });
            Command.Messages.Add(new Goal() { GoalId = 101 });

            // find valid players

            for (int player = 1; player <= 8; player++)
            {
                Command.Messages.Add(new PlayerValid() { PlayerNumber = player });
            }

            // update known elements

            foreach (var player in Players.Values)
            {
                player.RequestUpdate();
            }

            if (Tiles.Count > 0)
            {
                var tile_time = DateTime.UtcNow - TimeSpan.FromMinutes(3);
                if (GameTime < TimeSpan.FromMinutes(5))
                {
                    tile_time = DateTime.UtcNow - TimeSpan.FromMinutes(0.3);
                }
                else if (GameTime < TimeSpan.FromMinutes(10))
                {
                    tile_time = DateTime.UtcNow - TimeSpan.FromMinutes(1);
                }

                var tiles = Tiles.Values.ToList();
                tiles.Sort((a, b) =>
                {
                    var ca = a.LastUpdate < tile_time;
                    var cb = b.LastUpdate < tile_time;

                    if (ca && !cb)
                    {
                        return -1;
                    }
                    else if (cb && !ca)
                    {
                        return 1;
                    }
                    else
                    {
                        if (b.Explored && !a.Explored)
                        {
                            return -1;
                        }
                        else if (a.Explored && !b.Explored)
                        {
                            return 1;
                        }
                        else
                        {
                            var da = a.Position.DistanceTo(MyPosition);
                            var db = b.Position.DistanceTo(MyPosition);

                            return da.CompareTo(db);
                        }
                    }
                });

                for (int i = 0; i < Math.Min(tiles.Count, TILES_PER_COMMAND); i++)
                {
                    tiles[i].RequestUpdate();
                }
            }

            if (Units.Count > 0)
            {
                var units = Units.Values.ToList();
                units.Sort((a, b) => a.LastUpdate.CompareTo(b.LastUpdate));
                var count = Math.Min(units.Count, UNITS_PER_COMMAND / 2);

                for (int i = 0; i < count; i++)
                {
                    units[i].RequestUpdate();
                }

                for (int i = 0; i < count; i++)
                {
                    units[RNG.Next(units.Count)].RequestUpdate();
                }
            }

            if (UnitTypeInfos.Count > 0)
            {
                var infos = UnitTypeInfos.Values.ToList();
                infos.Sort((a, b) => a.LastUpdate.CompareTo(b.LastUpdate));
                var count = Math.Min(infos.Count, UNITTYPEINFOS_PER_COMMAND / 2);

                for (int i = 0; i < count; i++)
                {
                    infos[i].RequestUpdate();
                }

                for (int i = 0; i < count; i++)
                {
                    infos[RNG.Next(infos.Count)].RequestUpdate();
                }
            }

            return new List<Command>() { Command };
        }

        internal void Update()
        {
            if (Command.Messages.Count == 0)
            {
                return;
            }

            Debug.Assert(Command.Responses.Count == Command.Messages.Count);

            // general info

            GameTime = TimeSpan.FromSeconds(Command.Responses[0].Unpack<GameTimeResult>().Result);
            var x = Command.Responses[2].Unpack<GoalResult>().Result;
            var y = Command.Responses[3].Unpack<GoalResult>().Result;
            MyPosition = new Position(x, y);
            MapWidthHeight = Command.Responses[7].Unpack<GoalResult>().Result + 1;

            // find valid players

            for (int player = 1; player <= 8; player++)
            {
                var valid = Command.Responses[8 + player].Unpack<PlayerValidResult>().Result;

                if (valid && !_Players.ContainsKey(player))
                {
                    _Players.Add(player, new Player(player));
                }
            }

            foreach (var player in Players.Values)
            {
                player.Update();
            }

            foreach (var tile in Tiles.Values)
            {
                tile.Update();
            }

            foreach (var unit in Units.Values)
            {
                unit.Update();
            }

            foreach (var info in UnitTypeInfos.Values)
            {
                info.Update();
            }

            Command.Messages.Clear();
            Command.Responses.Clear();
        }
    }
}