﻿using AoE2Lib.Bots.GameElements;
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
        public int Width { get; private set; } = -1;
        public int Height { get; private set; } = -1;
        public bool AutoUpdate { get; set; } = true;

        private Tile[] Tiles { get; set; }
        private readonly Command Command = new Command();
        private readonly Random RNG = new Random(Guid.NewGuid().GetHashCode());

        public bool IsOnMap(Position position)
        {
            return IsOnMap(position.PointX, position.PointY);
        }

        public bool IsOnMap(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public Tile GetTile(Position position)
        {
            return GetTile(position.PointX, position.PointY);
        }

        public Tile GetTile(int x, int y)
        {
            if (!IsOnMap(x, y))
            {
                return null;
            }

            var index = (x * Height) + y;
            return Tiles[index];
        }

        private void SetTile(int x, int y, Tile tile)
        {
            var index = (x * Height) + y;
            Tiles[index] = tile;
        }

        public IEnumerable<Tile> GetTiles()
        {
            if (Tiles != null)
            {
                return Tiles;
            }
            else
            {
                return Enumerable.Empty<Tile>();
            }
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
                if (Tiles != null)
                {
                    if (Tiles.Length != (Width * Height))
                    {
                        Tiles = null;
                    }
                    else if (Tiles.Length == 0)
                    {
                        Tiles = null;
                    }
                }
                
                if (Tiles == null)
                {
                    Tiles = new Tile[Width * Height];
                    for (int x = 0; x < Width; x++)
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            SetTile(x, y, new Tile(Bot, new Point(x, y)));
                        }
                    }
                }
            }
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

            if (Tiles != null)
            {
                var positions = new List<Position>();
                positions.AddRange(Bot.GetModule<UnitsModule>().Units.Values.Where(u => u.PlayerNumber == Bot.PlayerNumber).Select(u => u.Position));
                positions.Add(Bot.GetModule<InfoModule>().MyPosition);

                var gametime = Bot.GetModule<InfoModule>().GameTime;
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
                    var position = positions[RNG.Next(positions.Count)];
                    foreach (var tile in GetTilesInRange(position, 20))
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

                foreach (var tile in tiles)
                {
                    tile.RequestUpdate();
                }
            }
        }
    }
}
