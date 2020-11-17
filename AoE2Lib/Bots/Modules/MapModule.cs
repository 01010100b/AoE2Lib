using AoE2Lib.Bots.GameElements;
using AoE2Lib.Utils;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AoE2Lib.Bots.Modules
{
    public class MapModule : Module
    {
        public int Width { get; private set; } = -1;
        public int Height { get; private set; } = -1;

        private Tile[][] Tiles { get; set; }
        private readonly Command Command = new Command();
        private readonly Random RNG = new Random(Guid.NewGuid().GetHashCode() ^ DateTime.UtcNow.GetHashCode());

        public bool IsOnMap(Vector2 position)
        {
            return IsOnMap(position.PointX, position.PointY);
        }

        public bool IsOnMap(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public Tile GetTile(Vector2 position)
        {
            return GetTile(position.PointX, position.PointY);
        }

        public Tile GetTile(int x, int y)
        {
            if (!IsOnMap(x, y))
            {
                return null;
            }

            return Tiles[x][y];
        }

        public IEnumerable<Tile> GetTiles()
        {
            if (Tiles == null)
            {
                return Enumerable.Empty<Tile>();
            }

            return Tiles.SelectMany(r => r);
        }

        public IEnumerable<Tile> GetTilesByDistance(Vector2 position)
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

            foreach (var tile in GetTiles())
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
                    foreach (var tile in GetTiles())
                    {
                        tile.Update();
                    }

                    if (Tiles.Length != Width)
                    {
                        Tiles = null;
                    }
                    else if (Tiles.Length == 0)
                    {
                        Tiles = null;
                    }
                    else if (Tiles[0].Length != Height)
                    {
                        Tiles = null;
                    }
                }
                
                if (Tiles == null)
                {
                    Tiles = new Tile[Width][];
                    for (int x = 0; x < Width; x++)
                    {
                        Tiles[x] = new Tile[Height];

                        for (int y = 0; y < Height; y++)
                        {
                            Tiles[x][y] = new Tile(Bot, x, y);
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
                    var units = Bot.GetModule<UnitsModule>().MyUnits;

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
