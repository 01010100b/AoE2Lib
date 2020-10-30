using Unary.GameElements;
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
        public TimeSpan GameTime { get; internal set; } = TimeSpan.Zero;
        public Position MyPosition { get; internal set; } = new Position(-1, -1);
        public int MapWidthHeight
        {
            get
            {
                return _MapWidthHeight;
            }
            internal set
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

        internal readonly Command Command = new Command();

        public void AddUnit(int id)
        {
            if (!_Units.ContainsKey(id))
            {
                _Units.Add(id, new Unit(id));
            }
        }

        internal void RequestUpdate(Bot bot)
        {
            Command.Messages.Clear();
            Command.Responses.Clear();

            Command.Messages.Add(new GameTime());
            Command.Messages.Add(new UpGetPoint() { PositionType = 9, GoalPoint = 100 });
            Command.Messages.Add(new Goal() { GoalId = 100 });
            Command.Messages.Add(new Goal() { GoalId = 101 });
            Command.Messages.Add(new SetGoal() { GoalId = 50, GoalValue = 1000 });
            Command.Messages.Add(new SetGoal() { GoalId = 51, GoalValue = 1000 });
            Command.Messages.Add(new UpBoundPoint() { GoalPoint1 = 100, GoalPoint2 = 50 });
            Command.Messages.Add(new Goal() { GoalId = 100 });
            Command.Messages.Add(new Goal() { GoalId = 101 });

            for (int player = 1; player <= 2; player++)
            {
                if (!_Players.ContainsKey(player))
                {
                    _Players.Add(player, new Player(player));
                }
            }

            foreach (var player in Players.Values)
            {
                player.RequestUpdate();
            }

            if (Tiles.Count > 0)
            {
                var tiles = Tiles.Values.ToList();
                for (int i = 0; i < 100; i++)
                {
                    var tile = tiles[bot.RNG.Next(tiles.Count)];
                    tile.RequestUpdate();
                }
            }
        }

        internal void Update()
        {
            if (Command.Messages.Count == 0)
            {
                return;
            }

            Debug.Assert(Command.Responses.Count == Command.Messages.Count);

            GameTime = TimeSpan.FromSeconds(Command.Responses[0].Unpack<GameTimeResult>().Result);
            var x = Command.Responses[2].Unpack<GoalResult>().Result;
            var y = Command.Responses[3].Unpack<GoalResult>().Result;
            MyPosition = new Position(x, y);
            MapWidthHeight = Command.Responses[7].Unpack<GoalResult>().Result + 1;

            foreach (var player in Players.Values)
            {
                player.Update();
            }

            foreach (var tile in Tiles.Values)
            {
                tile.Update();
            }

            Command.Messages.Clear();
            Command.Responses.Clear();
        }
    }
}
