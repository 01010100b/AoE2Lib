using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AoE2Lib.Bots.GameElements
{
    public class Tile
    {
        private static readonly Point[] NEIGHBOURS = { new Point(-1, 0), new Point(1, 0), new Point(0, -1), new Point(0, 1) };
        private static readonly Point[] DIAGONAL_NEIGHTBOURS = { new Point(-1, -1), new Point(-1, 1), new Point(1, -1), new Point(1, 1) };

        public readonly int X;
        public readonly int Y;
        public Position Position => Position.FromPoint(X, Y);
        public Position Center => new Position(X + 0.5, Y + 0.5);
        public int Height { get; internal set; }
        public bool IsOnLand => Terrain != 1 && Terrain != 2 && Terrain != 4 && Terrain != 15 && Terrain != 22 && Terrain != 23 && Terrain != 28 && Terrain != 37;
        public bool Explored => Visibility != 0;
        public bool Visible => Visibility == 15;
        public IEnumerable<Unit> Units => _Units;

        internal readonly List<Unit> _Units = new List<Unit>();
        internal int Terrain { get; set; } = 0;
        internal int Visibility { get; set; } = 0;

        private readonly Map Map;
        private readonly List<Tile> Neighbours = new List<Tile>();
        private readonly List<Tile> AllNeighbours = new List<Tile>();

        internal Tile(int x, int y, Map map)
        {
            X = x;
            Y = y;
            Map = map;
        }

        public IReadOnlyList<Tile> GetNeighbours(bool include_diagonal = false)
        {
            if (Neighbours.Count == 0)
            {
                foreach (var delta in NEIGHBOURS)
                {
                    var x = X + delta.X;
                    var y = Y + delta.Y;

                    if (Map.TryGetTile(x, y, out var t))
                    {
                        Neighbours.Add(t);
                        AllNeighbours.Add(t);
                    }
                }

                foreach (var delta in DIAGONAL_NEIGHTBOURS)
                {
                    var x = X + delta.X;
                    var y = Y + delta.Y;

                    if (Map.TryGetTile(x, y, out var t))
                    {
                        AllNeighbours.Add(t);
                    }
                }
            }

            if (include_diagonal)
            {
                return AllNeighbours;
            }
            else
            {
                return Neighbours;
            }
        }
    }
}
