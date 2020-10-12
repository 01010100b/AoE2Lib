using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;

namespace AoE2Lib.Bots
{
    public class Tile : GameElement
    {
        public readonly Position Position; // 500 x 500
        public int UnitId { get; private set; } = -1; // 45000
        public int Elevation { get; private set; } = -1; // 64
        public int TerrainId { get; private set; } = -1; // 64
        public bool Explored { get; private set; } = false; // 2
        public IReadOnlyCollection<Unit> Units => _Units;
        internal readonly HashSet<Unit> _Units = new HashSet<Unit>();

        public Tile(Position position) : base()
        {
            Position = position;
        }

        internal void Update(int goal0, int goal1)
        {
            var x = goal0 / 500;
            var y = goal0 % 500;
            var position = new Position(x, y);

            if (position != Position)
            {
                throw new ArgumentException("Incorrect tile position: " + position);
            }

            UnitId = goal1 % 45000;
            goal1 /= 45000;
            Elevation = goal1 % 64;
            goal1 /= 64;
            TerrainId = goal1 % 64;
            goal1 /= 64;
            Explored = goal1 % 2 == 1;

            if (!Explored)
            {
                UnitId = -1;
                Elevation = -1;
                TerrainId = -1;
            }

            ElementUpdated();
        }
    }
}
