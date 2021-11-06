using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Protos.Expert.Action;
using Protos.Expert.Command;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;

namespace AoE2Lib.Bots.GameElements
{
    public class Tile
    {
        public static readonly Point[] NEIGHBOURS = new[] { new Point(-1, 0), new Point(1, 0), new Point(0, -1), new Point(0, 1) };

        public readonly int X;
        public readonly int Y;
        public int Height { get; internal set; }
        public Position Position => Position.FromPoint(X, Y);
        public Position Center => new Position(X + 0.5, Y + 0.5);
        public bool IsOnLand => Terrain != 1 && Terrain != 2 && Terrain != 4 && Terrain != 15 && Terrain != 22 && Terrain != 23 && Terrain != 28 && Terrain != 37;
        public bool Explored => Visibility != 0;
        public bool Visible => Visibility == 15;
        public readonly List<Unit> Units = new List<Unit>();

        internal int Terrain { get; set; } = 0;
        internal int Visibility { get; set; } = 0;
        private readonly Map Map;

        internal Tile(int x, int y, Map map)
        {
            X = x;
            Y = y;
            Map = map;
        }

        public IEnumerable<Tile> GetNeighbours()
        {
            foreach (var delta in NEIGHBOURS)
            {
                var x = X + delta.X;
                var y = Y + delta.Y;

                if (Map.IsOnMap(x, y))
                {
                    var t = Map.GetTile(x, y);
                    yield return t;
                }
            }
        }
    }

    public class Map : GameElement
    {
        public int Height { get; private set; } = -1;
        public int Width { get; private set; } = -1;

        private Tile[] Tiles { get; set; }

        public Map(Bot bot) : base(bot)
        {

        }

        public bool IsOnMap(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public bool IsOnMap(Position position)
        {
            return IsOnMap(position.PointX, position.PointY);
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

        public Tile GetTile(Position position)
        {
            return GetTile(position.PointX, position.PointY);
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

        public IEnumerable<Tile> GetTilesInRange(Position position, double range)
        {
            var r = Math.Max(0, (int)Math.Ceiling(range) + 1);

            for (int cx = Math.Max(0, position.PointX - r); cx <= Math.Min(Width - 1, position.PointX + r); cx++)
            {
                for (int cy = Math.Max(0, position.PointY - r); cy <= Math.Min(Height - 1, position.PointY + r); cy++)
                {
                    if (IsOnMap(cx, cy))
                    {
                        var tile = GetTile(cx, cy);

                        if (position.DistanceTo(tile.Center) <= range)
                        {
                            yield return GetTile(cx, cy);
                        }
                    }
                }
            }
        }

        protected override IEnumerable<IMessage> RequestElementUpdate()
        {
            yield return new GetMapDimensions();
            yield return new GetTiles();
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
                    Tiles[index] = new Tile(x, y, this);
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
