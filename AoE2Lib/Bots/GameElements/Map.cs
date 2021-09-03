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
        public int Height;
        internal int Terrain;
        internal int Visibility;
        public readonly List<Unit> Units = new List<Unit>();
        public Position Position => Position.FromPoint(X, Y);

        public bool IsOnLand => Terrain != 1 && Terrain != 2 && Terrain != 4 && Terrain != 15 && Terrain != 22 && Terrain != 23 && Terrain != 28 && Terrain != 37;
        public bool Explored => Visibility != 0;
        public bool Visible => Visibility == 15;

        internal Tile(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class Map : GameElement
    {
        private struct BuildPosition
        {
            public readonly UnitType UnitType;
            public readonly Tile Tile;

            public BuildPosition(UnitType building, Tile tile)
            {
                UnitType = building;
                Tile = tile;
            }
        }

        public int Height { get; private set; } = -1;
        public int Width { get; private set; } = -1;

        private Tile[] Tiles { get; set; }
        private readonly Dictionary<Tile, bool> ReachableTiles = new Dictionary<Tile, bool>();
        private readonly List<Tile> CheckReachableTiles = new List<Tile>();

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
            return GetTilesInRange(0, 0, Width + Height);
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

        public bool TryCanReachPosition(int x, int y, out bool can_reach)
        {
            var tile = GetTile(x, y);

            if (!tile.Explored)
            {
                can_reach = false;

                return false;
            }

            if (ReachableTiles.TryGetValue(tile, out can_reach))
            {
                return true;
            }
            else
            {
                CheckReachableTiles.Add(tile);

                return false;
            }
        }

        protected override IEnumerable<IMessage> RequestElementUpdate()
        {
            yield return new GetMapDimensions();
            yield return new GetTiles();

            foreach (var tile in ReachableTiles.Keys.ToList())
            {
                if (Bot.Rng.NextDouble() < 0.01)
                {
                    ReachableTiles.Remove(tile);
                }
            }

            var checked_tiles = new List<Tile>();
            foreach (var tile in CheckReachableTiles.Distinct().Where(t => !ReachableTiles.ContainsKey(t)))
            {
                checked_tiles.Add(tile);

                if (checked_tiles.Count >= 100)
                {
                    break;
                }
            }

            CheckReachableTiles.Clear();
            CheckReachableTiles.AddRange(checked_tiles);

            Bot.Log.Debug($"Check can reach {CheckReachableTiles.Count} tiles with {ReachableTiles.Count} cached.");

            if (CheckReachableTiles.Count == 0)
            {
                yield break;
            }

            yield return new UpFullResetSearch();
            yield return new UpFindLocal() { InConstUnitId = 904, InConstCount = 1 };
            yield return new UpSetTargetObject() { InConstIndex = 0, InConstSearchSource = 1 };

            foreach (var tile in CheckReachableTiles)
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

            var index = 7;
            foreach (var tile in CheckReachableTiles)
            {
                var can_reach = responses[index].Unpack<UpPathDistanceResult>().Result != 65535;
                ReachableTiles[tile] = can_reach;

                index += 3;
            }

            CheckReachableTiles.Clear();
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
