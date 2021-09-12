using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Protos.Expert.Action;
using Protos.Expert.Command;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace AoE2Lib.Bots.GameElements
{
    public class Tile
    {
        public readonly int X;
        public readonly int Y;
        public int Height { get; internal set; }
        public Position Position => Position.FromPoint(X, Y);
        public bool IsOnLand => Terrain != 1 && Terrain != 2 && Terrain != 4 && Terrain != 15 && Terrain != 22 && Terrain != 23 && Terrain != 28 && Terrain != 37;
        public bool Explored => Visibility != 0;
        public bool Visible => Visibility == 15;
        public readonly List<Unit> Units = new List<Unit>();

        internal int Terrain { get; set; }
        internal int Visibility { get; set; }

        internal Tile(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class Map : GameElement
    {
        public int Height { get; private set; } = -1;
        public int Width { get; private set; } = -1;

        private Tile[] Tiles { get; set; }
        private readonly Dictionary<Tile, int> PathDistances = new Dictionary<Tile, int>();
        private readonly List<Tile> CheckPathDistances = new List<Tile>();

        public Map(Bot bot) : base(bot)
        {
        }

        public bool IsOnMap(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public Tile GetTile(int x, int y)
        {
            if (!IsOnMap(x, y))
            {
                throw new IndexOutOfRangeException($"Tile {x},{y} is not on map");
            }
            else
            {
                return Tiles[GetIndex(x, y)];
            }
        }

        public IEnumerable<Tile> GetTiles()
        {
            if (Tiles == null)
            {
                return Enumerable.Empty<Tile>();
            }
            else
            {
                return Tiles;
            }
        }

        public IEnumerable<Tile> GetTilesInRange(int x, int y, double range)
        {
            var r = Math.Max(0, (int)Math.Ceiling(range) + 1);
            var pos = Position.FromPoint(x, y);

            for (int cx = Math.Max(0, x - r); cx <= Math.Min(Width - 1, x + r); cx++)
            {
                for (int cy = Math.Max(0, y - r); cy <= Math.Min(Height - 1, y + r); cy++)
                {
                    var p = Position.FromPoint(cx, cy);

                    if (pos.DistanceTo(p) <= range)
                    {
                        yield return GetTile(cx, cy);
                    }
                }
            }
        }

        public bool TryCanReach(int x, int y, out bool can_reach)
        {
            if (TryGetPathDistance(x, y, out int dist))
            {
                can_reach = dist != 65535;

                return true;
            }
            else
            {
                can_reach = false;

                return false;
            }
        }

        public bool TryGetPathDistance(int x, int y, out int distance)
        {
            if (!IsOnMap(x, y))
            {
                distance = -1;

                return false;
            }

            var tile = GetTile(x, y);

            if (!tile.Explored)
            {
                distance = -1;

                return false;
            }

            if (PathDistances.TryGetValue(tile, out int dist))
            {
                distance = dist;

                return true;
            }
            else
            {
                CheckPathDistances.Add(tile);
                distance = -1;

                return false;
            }
        }

        protected override IEnumerable<IMessage> RequestElementUpdate()
        {
            yield return new GetMapDimensions();
            yield return new GetTiles();

            foreach (var tile in PathDistances.Keys.ToList())
            {
                if (Bot.GameState.Tick % 101 == tile.GetHashCode() % 101)
                {
                    PathDistances.Remove(tile);
                }
            }

            var checked_tiles = new List<Tile>();
            foreach (var tile in CheckPathDistances.Distinct().Where(t => !PathDistances.ContainsKey(t)))
            {
                checked_tiles.Add(tile);

                if (checked_tiles.Count >= 100)
                {
                    break;
                }
            }

            CheckPathDistances.Clear();
            CheckPathDistances.AddRange(checked_tiles);

            Bot.Log.Debug($"Check {CheckPathDistances.Count} path distances with {PathDistances.Count} cached.");

            if (CheckPathDistances.Count == 0)
            {
                yield break;
            }

            yield return new UpFullResetSearch();
            yield return new SetGoal() { InConstGoalId = 100, InConstValue = Bot.GameState.MyPosition.PointX };
            yield return new SetGoal() { InConstGoalId = 101, InConstValue = Bot.GameState.MyPosition.PointY };
            yield return new UpSetTargetPoint() { InGoalPoint = 100 };
            yield return new UpFindLocal() { InConstUnitId = 904, InConstCount = 240 };
            yield return new UpCleanSearch() { InConstSearchSource = 1, InConstObjectData = (int)ObjectData.DISTANCE, InConstSearchOrder = 1 };
            yield return new UpSetTargetObject() { InConstIndex = 0, InConstSearchSource = 1 };

            foreach (var tile in CheckPathDistances)
            {
                yield return new SetGoal() { InConstGoalId = 100, InConstValue = tile.X };
                yield return new SetGoal() { InConstGoalId = 101, InConstValue = tile.Y };
                yield return new UpPathDistance() { InGoalPoint = 100, InConstStrict = 1 };
            }
        }

        protected override void UpdateElement(IReadOnlyList<Any> responses)
        {
            var dims = responses[0].Unpack<GetMapDimensionsResult>();
            Height = dims.Height;
            Width = dims.Width;
            CreateMap();
            
            var tiles = responses[1].Unpack<GetTilesResult>().Tiles;
            foreach (var tile in tiles)
            {
                var t = GetTile(tile.X, tile.Y);
                t.Visibility = tile.Visibility;
                if (tile.Visibility != 0)
                {
                    t.Height = tile.Height;
                    t.Terrain = tile.Terrain;
                }
            }

            var index = 11;
            foreach (var tile in CheckPathDistances)
            {
                PathDistances[tile] = responses[index].Unpack<UpPathDistanceResult>().Result;
                index += 3;
            }

            CheckPathDistances.Clear();
        }

        private void CreateMap()
        {
            if (Tiles != null && Tiles.Length == Width * Height)
            {
                return;
            }

            if (Width <= 0 || Height <= 0)
            {
                return;
            }

            Tiles = new Tile[Width * Height];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var index = GetIndex(x, y);
                    Tiles[index] = new Tile(x, y);
                }
            }

            Bot.Log.Debug($"Map width {Width} height {Height}");
        }

        private int GetIndex(int x, int y)
        {
            return (x * Height) + y;
        }
    }
}
