﻿using AoE2Lib.Bots.GameElements;
using AoE2Lib.Utils;
using Protos.Expert.Action;
using Protos.Expert.Fact;
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
        public struct Tile
        {
            public readonly Point Point;
            public Position Position => Position.FromPoint(Point.X, Point.Y);
            public bool Explored { get; private set; }
            public int Elevation { get; private set; }
            public int Terrain { get; private set; }
            public bool IsOnLand => Terrain != 1 && Terrain != 2 && Terrain != 4 && Terrain != 15 && Terrain != 22 && Terrain != 23 && Terrain != 28 && Terrain != 37;
            
            internal readonly Command Command;
            internal TimeSpan LastUpdateGameTime { get; private set; }

            internal Tile(Point point)
            {
                Point = point;
                Explored = false;
                Elevation = -1;
                Terrain = -1;
                Command = new Command();
                LastUpdateGameTime = TimeSpan.MinValue;
            }

            public void RequestUpdate()
            {
                Command.Reset();

                Command.Add(new SetGoal() { GoalId = 50, GoalValue = Point.X });
                Command.Add(new SetGoal() { GoalId = 51, GoalValue = Point.Y });
                Command.Add(new UpBoundPoint() { GoalPoint1 = 52, GoalPoint2 = 50 });
                Command.Add(new UpPointElevation() { GoalPoint = 52 });
                Command.Add(new UpPointTerrain() { GoalPoint = 52 });
                Command.Add(new UpPointExplored() { GoalPoint = 52 });
            }

            internal void Update(TimeSpan gametime)
            {
                var responses = Command.GetResponses();
                
                if (responses.Count > 0)
                {
                    Elevation = responses[3].Unpack<UpPointElevationResult>().Result;
                    Terrain = responses[4].Unpack<UpPointTerrainResult>().Result;
                    Explored = responses[5].Unpack<UpPointExploredResult>().Result != 0;
                    LastUpdateGameTime = gametime;

                    Command.Reset();
                }
            }
        }

        public int Width { get; private set; } = -1;
        public int Height { get; private set; } = -1;

        private Tile[,] Tiles { get; set; }
        private readonly Command Command = new Command();
        private readonly Random RNG = new Random(Guid.NewGuid().GetHashCode() ^ DateTime.UtcNow.GetHashCode());

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
                return new Tile(new Point(-1, -1));
            }

            return Tiles[x, y];
        }

        private void SetTile(int x, int y, Tile tile)
        {
            Tiles[x, y] = tile;
        }

        public IEnumerable<Tile> GetTiles()
        {
            if (Tiles != null)
            {
                foreach (var tile in Tiles)
                {
                    yield return tile;
                }
            }
        }

        public IEnumerable<Tile> GetTilesByDistance(Position position)
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
        
        protected override IEnumerable<Command> RequestUpdate()
        {
            Command.Reset();

            Command.Add(new SetGoal() { GoalId = 50, GoalValue = 10000 });
            Command.Add(new SetGoal() { GoalId = 51, GoalValue = 10000 });
            Command.Add(new UpBoundPoint() { GoalPoint1 = 52, GoalPoint2 = 50 });
            Command.Add(new Goal() { GoalId = 52 });
            Command.Add(new Goal() { GoalId = 53 });

            yield return Command;

            AddDefaultCommands();

            foreach (var tile in GetTiles().Where(t => t.Command.HasMessages))
            {
                yield return tile.Command;
            }
        }

        protected override void Update()
        {
            var responses = Command.GetResponses();
            if (responses.Count > 0)
            {
                Width = responses[3].Unpack<GoalResult>().Result + 1;
                Height = responses[4].Unpack<GoalResult>().Result + 1;
            }

            if (Width > 0 && Height > 0)
            {
                if (Tiles != null)
                {
                    var gametime = Bot.GetModule<InfoModule>().GameTime;

                    foreach (var tile in GetTiles())
                    {
                        if (tile.Command.HasResponses)
                        {
                            tile.Update(gametime);
                            SetTile(tile.Point.X, tile.Point.Y, tile);
                        }
                        
                    }

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
                    Tiles = new Tile[Width, Height];
                    for (int x = 0; x < Width; x++)
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            Tiles[x, y] = new Tile(new Point(x, y));
                        }
                    }
                }
            }
        }

        private void AddDefaultCommands()
        {
            const int TILES_PER_COMMAND = 50;

            if (Tiles != null)
            {
                var position = Bot.GetModule<InfoModule>().MyPosition;

                if (RNG.NextDouble() < 0.5)
                {
                    var units = Bot.GetModule<UnitsModule>().Units.Values.Where(u => u.PlayerNumber == Bot.PlayerNumber).ToList();

                    if (units.Count > 0)
                    {
                        position = units[RNG.Next(units.Count)].Position;
                    }
                }

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

                var tiles = new List<Tile>(TILES_PER_COMMAND);
                foreach (var tile in GetTilesByDistance(position))
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

                tiles.Sort((a, b) =>
                {
                    var da = a.Position.DistanceTo(position);
                    var db = b.Position.DistanceTo(position);

                    return da.CompareTo(db);
                });

                for (int i = 0; i < Math.Min(tiles.Count, TILES_PER_COMMAND); i++)
                {
                    tiles[i].RequestUpdate();
                }
            }
        }
    }
}
