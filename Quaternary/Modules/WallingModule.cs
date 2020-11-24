using AoE2Lib.Bots;
using AoE2Lib.Utils;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Quaternary.Modules.MapAnalysisModule;

namespace Quaternary.Modules
{
    class WallingModule : Module
    {
        public class Wall
        {
            // this will be set to true after the wall has been generated
            public bool IsGenerated { get; private set; }

            // the following lists will be filled when the wall is generated
            public readonly List<AnalysisTile> Pieces = new List<AnalysisTile>();
            public readonly List<KeyValuePair<AnalysisTile, int>> Gates = new List<KeyValuePair<AnalysisTile, int>>();

            internal readonly List<Point> Goals = new List<Point>();
            internal readonly int Optimizations;
            
            private AnalysisTile[] Tiles;
            private Point Offset;

            internal Wall(IEnumerable<Point> goals, int optimizations)
            {
                Goals.AddRange(goals);
                Optimizations = optimizations;
            }

            internal AnalysisTile GetTile(Point point)
            {
                var index = GetIndex(point);

                return Tiles[index];
            }

            internal void SetTile(AnalysisTile tile)
            {
                var index = GetIndex(tile.Point);
                Tiles[index] = tile;
            }

            internal void SetSize(int size)
            {
                Tiles = new AnalysisTile[size * size];
            }

            internal void SetOffset(Point offset)
            {
                Offset = offset;
            }

            private int GetIndex(Point point)
            {
                var p = new Point(point.X - Offset.X, point.Y - Offset.Y);

                return ((int)Math.Round(Math.Sqrt(Tiles.Length)) * p.X) + p.Y;
            }
        }

        public Wall GetWall(IEnumerable<Point> goals, int optimizations)
        {
            var map = Bot.GetModule<MapAnalysisModule>();
            var wall = new Wall(goals.Where(g => map.IsOnMap(g)), optimizations);

            if (wall.Goals.Count == 0)
            {
                return null;
            }

            var center = Position.Zero;
            foreach (var goal in wall.Goals)
            {
                center += goal;
            }
            center /= wall.Goals.Count;

            var range = wall.Goals.Max(g => center.DistanceTo(g));
            var size = (int)Math.Ceiling(range * 4);
            if (size < 10)
            {
                size = 10;
            }

            wall.SetSize(size);
            var offset = new Point(center.PointX - (size / 2), center.PointY - (size / 2));
            wall.SetOffset(offset);
            for (int x = offset.X; x < offset.X + size; x++)
            {
                for (int y = offset.Y; y < offset.Y + size; y++)
                {
                    wall.SetTile(map.GetTile(new Point(x, y)));
                }
            }

            return wall;
        }

        protected override IEnumerable<Command> RequestUpdate()
        {
            return Enumerable.Empty<Command>();
        }

        protected override void Update()
        {

        }

        private void GenerateWall(Wall wall)
        {

        }
    }
}
