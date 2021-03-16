using AoE2Lib.Bots.GameElements;
using AoE2Lib.Utils;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Protos.Expert.Action;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AoE2Lib.Bots.Modules
{
    public class MapModule : Module
    {
        public IEnumerable<Tile> Tiles => _Tiles;
        public Tile this[Position position] { get { return GetTile(position.PointX, position.PointY); } }
        public int Width { get; private set; } = -1;
        public int Height { get; private set; } = -1;
        public bool AutoUpdate { get; set; } = true;

        private Tile[] _Tiles { get; set; } = new Tile[0];
        private readonly Command Command = new Command();

        public bool IsOnMap(Position position)
        {
            return IsOnMap(position.PointX, position.PointY);
        }

        public IEnumerable<Tile> GetTilesInRange(Position position, double range)
        {
            foreach (var tile in GetTilesByDistance(position))
            {
                if (tile.Position.DistanceTo(position) > 1.5 * range)
                {
                    yield break;
                }

                if (tile.Position.DistanceTo(position) <= range)
                {
                    yield return tile;
                }
            }
        }
        
        protected override IEnumerable<Command> RequestUpdate()
        {
            Command.Reset();

            Command.Add(new SetGoal() { InConstGoalId = 50, InConstValue = 10000 });
            Command.Add(new SetGoal() { InConstGoalId = 51, InConstValue = 10000 });
            Command.Add(new UpBoundPoint() { OutGoalPoint = 52, InGoalPoint = 50 });
            Command.Add(new Protos.Expert.Fact.Goal() { InConstGoalId = 52 });
            Command.Add(new Protos.Expert.Fact.Goal() { InConstGoalId = 53 });

            yield return Command;

            if (AutoUpdate)
            {
                AddDefaultCommands();
            }
        }

        protected override void Update()
        {
            if (Command.HasResponses)
            {
                var responses = Command.GetResponses();

                Width = responses[3].Unpack<Protos.Expert.Fact.GoalResult>().Result + 1;
                Height = responses[4].Unpack<Protos.Expert.Fact.GoalResult>().Result + 1;
            }
             
            if (Width > 0 && Height > 0)
            {
                if (_Tiles != null)
                {
                    if (_Tiles.Length != (Width * Height))
                    {
                        _Tiles = null;
                    }
                    else if (_Tiles.Length == 0)
                    {
                        _Tiles = null;
                    }
                }
                
                if (_Tiles == null)
                {
                    _Tiles = new Tile[Width * Height];
                    for (int x = 0; x < Width; x++)
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            SetTile(x, y, new Tile(Bot, new Point(x, y)));
                        }
                    }
                }
            }

            foreach (var tile in Tiles)
            {
                tile.UnitsInternal.Clear();
            }

            var units = Bot.UnitsModule;
            foreach (var unit in units.Units.Values.Where(u => IsOnMap(u.Position)))
            {
                this[unit.Position].UnitsInternal.Add(unit);
            }
        }

        private bool IsOnMap(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        private Tile GetTile(int x, int y)
        {
            if (!IsOnMap(x, y))
            {
                return null;
            }

            var index = (x * Height) + y;
            return _Tiles[index];
        }

        private void SetTile(int x, int y, Tile tile)
        {
            var index = (x * Height) + y;
            _Tiles[index] = tile;
        }

        private IEnumerable<Tile> GetTilesByDistance(Position position)
        {
            var pointx = position.PointX;
            var pointy = position.PointY;

            if (!IsOnMap(pointx, pointy))
            {
                yield break;
            }

            var max = Math.Max(Width, Height);
            for (int delta = 0; delta <= max; delta++)
            {
                int x, y;

                x = pointx - delta;
                if (x >= 0 && x < Width)
                {
                    for (y = pointy - delta; y <= pointy + delta; y++)
                    {
                        if (y >= 0 && y < Height)
                        {
                            yield return GetTile(x, y);
                        }
                    }
                }

                if (delta > 0)
                {
                    x = pointx + delta;
                    if (x >= 0 && x < Width)
                    {
                        for (y = pointy - delta; y <= pointy + delta; y++)
                        {
                            if (y >= 0 && y < Height)
                            {
                                yield return GetTile(x, y);
                            }
                        }
                    }

                    y = pointy - delta;
                    if (y >= 0 && y < Height)
                    {
                        for (x = pointx - delta + 1; x <= pointx + delta - 1; x++)
                        {
                            if (x >= 0 && x < Width)
                            {
                                yield return GetTile(x, y);
                            }
                        }
                    }

                    y = pointy + delta;
                    if (y >= 0 && y < Height)
                    {
                        for (x = pointx - delta + 1; x <= pointx + delta - 1; x++)
                        {
                            if (x >= 0 && x < Width)
                            {
                                yield return GetTile(x, y);
                            }
                        }
                    }
                }
            }
        }

        private void AddDefaultCommands()
        {
            const int TILES_PER_COMMAND = 100;

            if (_Tiles != null)
            {
                var positions = new List<Position>();
                positions.AddRange(Bot.UnitsModule.Units.Values.Where(u => u.PlayerNumber == Bot.PlayerNumber).Select(u => u.Position));
                positions.Add(Bot.InfoModule.MyPosition);

                var gametime = Bot.InfoModule.GameTime;
                var tile_time = gametime - TimeSpan.FromMinutes(3);

                if (gametime < TimeSpan.FromMinutes(5))
                {
                    tile_time = gametime - TimeSpan.FromMinutes(0.3);
                }
                else if (gametime < TimeSpan.FromMinutes(10))
                {
                    tile_time = gametime - TimeSpan.FromMinutes(1);
                }

                var tiles = new HashSet<Tile>();

                for (int i = 0; i < 10; i++)
                {
                    var position = positions[Bot.Rng.Next(positions.Count)];
                    foreach (var tile in GetTilesInRange(position, 30))
                    {
                        if (tile.LastUpdateGameTime < tile_time && !tile.Explored)
                        {
                            tiles.Add(tile);
                        }

                        if (tiles.Count >= TILES_PER_COMMAND)
                        {
                            break;
                        }
                    }

                    if (tiles.Count >= TILES_PER_COMMAND)
                    {
                        break;
                    }
                }

                for (int i = 0; i < TILES_PER_COMMAND; i++)
                {
                    var x = Bot.Rng.Next(Width);
                    var y = Bot.Rng.Next(Height);

                    var tile = GetTile(x, y);
                    if (tile.LastUpdateGameTime < tile_time && !tile.Explored)
                    {
                        tiles.Add(tile);
                    }
                }

                foreach (var tile in tiles)
                {
                    tile.RequestUpdate();
                }
            }
        }
    }
}
