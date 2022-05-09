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
    

    public class Map : GameElement
    {
        public int Width { get; private set; } = -1;
        public int Height { get; private set; } = -1;
        public Position Center => Position.FromPoint(Width / 2, Height / 2);

        private Tile[] Tiles { get; set; }

        public Map(Bot bot) : base(bot)
        {

        }

        public bool IsOnMap(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public bool IsOnMap(Position position) => IsOnMap(position.PointX, position.PointY);

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

        public bool TryGetTile(int x, int y, out Tile tile)
        {
            if (IsOnMap(x, y))
            {
                tile = Tiles[GetIndex(x, y)];

                return true;
            }
            else
            {
                tile = default;

                return false;
            }
        }

        public bool TryGetTile(Position position, out Tile tile) => TryGetTile(position.PointX, position.PointY, out tile);

        public IEnumerable<Tile> GetTilesInRange(Position position, double range)
        {
            var r = Math.Max(0, (int)Math.Ceiling(range) + 1);
            var x_min = Math.Max(0, position.PointX - r);
            var x_max = Math.Min(Width - 1, position.PointX + r);
            var y_min = Math.Max(0, position.PointY - r);
            var y_max = Math.Min(Height - 1, position.PointY + r);

            for (int x = x_min; x <= x_max; x++)
            {
                for (int y = y_min; y <= y_max; y++)
                {
                    if (TryGetTile(x, y, out var tile))
                    {
                        if (position.DistanceTo(tile.Center) <= range)
                        {
                            yield return tile;
                        }
                    }
                }
            }
        }

        public IEnumerable<Unit> GetUnitsInRange(Position position, double range) => GetTilesInRange(position, range).SelectMany(t => t.Units);

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

            if (Width <= 0 || Height <= 0)
            {
                return;
            }

            if (Tiles == null || Tiles.Length != Width * Height)
            {
                CreateMap();
            }
                
            var tiles = responses[1].Unpack<GetTilesResult>().Tiles;
            foreach (var tile in tiles)
            {
                if (TryGetTile(tile.X, tile.Y, out var t))
                {
                    t.Visibility = tile.Visibility;
                    if (tile.Visibility != 0)
                    {
                        t.Height = tile.Height;
                        t.Terrain = tile.Terrain;
                    }
                }
            }
        }

        private void CreateMap()
        {
            Tiles = new Tile[Width * Height];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var index = GetIndex(x, y);
                    Tiles[index] = new Tile(x, y, this);
                }
            }

            Bot.Log.Info($"Map width {Width} height {Height}");
        }

        private int GetIndex(int x, int y)
        {
            return (x * Height) + y;
        }
    }
}
