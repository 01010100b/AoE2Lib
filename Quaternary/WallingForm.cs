using AoE2Lib.Utils;
using Protos.Expert.Fact;
using Quaternary.Algorithms;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Quaternary.Algorithms.AnalysisMap;

namespace Quaternary
{
    public partial class WallingForm : Form
    {
        private const int TILE_SIZE = 16;
        private readonly AnalysisMap Map = new AnalysisMap();
        private List<Point> Goals = new List<Point>();
        private List<Point> Interior = new List<Point>();

        public WallingForm()
        {
            InitializeComponent();
            DoubleBuffered = true;
        }

        private void WallingForm_Paint(object sender, PaintEventArgs e)
        {
            if (Map.Size < 1)
            {
                return;
            }

            var size = Map.Size;

            var g = e.Graphics;
            var brush = Brushes.AntiqueWhite;
            var rect = new Rectangle(TILE_SIZE, TILE_SIZE, TILE_SIZE, TILE_SIZE);
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    brush = Brushes.AntiqueWhite;
                    switch (Map.Tiles[x, y].Type)
                    {
                        case AnalysisTileType.WALL: brush = Brushes.Red; break;
                        case AnalysisTileType.WOOD: brush = Brushes.Green; break;
                        case AnalysisTileType.FOOD: brush = Brushes.LightGreen; break;
                        case AnalysisTileType.GOLD: brush = Brushes.Yellow; break;
                        case AnalysisTileType.STONE: brush = Brushes.LightGray; break;
                        case AnalysisTileType.OBSTRUCTION: brush = Brushes.Black; break;
                    }

                    if (x == size / 2 && y == size / 2)
                    {
                        brush = Brushes.Orange;
                    }

                    rect = new Rectangle(x * TILE_SIZE, y * TILE_SIZE, TILE_SIZE, TILE_SIZE);
                    g.FillRectangle(brush, rect);
                    g.DrawRectangle(Pens.White, rect);
                }
            }

            if (CheckShowInterior.Checked)
            {
                var interior = FloodFill.GetInterior(Map.Center, p => Map.GetNeighbours(p), p => Map.Tiles[p.X, p.Y].Type == AnalysisTileType.NONE);
                foreach (var point in interior)
                {
                    brush = Brushes.DarkGray;
                    rect = new Rectangle(point.X * TILE_SIZE, point.Y * TILE_SIZE, TILE_SIZE, TILE_SIZE);
                    g.FillRectangle(brush, rect);
                    g.DrawRectangle(Pens.White, rect);
                }
            }

            if (CheckShowGoals.Checked)
            {
                foreach (var point in Goals)
                {
                    brush = Brushes.Blue;
                    rect = new Rectangle(point.X * TILE_SIZE, point.Y * TILE_SIZE, TILE_SIZE, TILE_SIZE);
                    g.FillRectangle(brush, rect);
                    g.DrawRectangle(Pens.White, rect);
                }
            }

            brush = Brushes.Orange;
            rect = new Rectangle((size / 2) * TILE_SIZE, (size / 2) * TILE_SIZE, TILE_SIZE, TILE_SIZE);
            g.FillRectangle(brush, rect);
            g.DrawRectangle(Pens.White, rect);
        }

        private void ButtonGenerate_Click(object sender, EventArgs e)
        {
            Sample(100);
        }

        private void Sample(int samples)
        {
            var wall = 0d;
            var interior = 0d;
            for (int i = 0; i < samples; i++)
            {
                wall += Generate().Count;
                interior += FloodFill.GetInterior(Map.Center, p => Map.GetNeighbours(p), p => Map.Tiles[p.X, p.Y].Type == AnalysisTileType.NONE).Count;
            }

            wall /= samples;
            interior /= samples;

            Debug.WriteLine($"Average wall {wall:N0} interior {interior:N0} effiency {interior / wall:N2}");
        }

        private List<Point> Generate()
        {
            Map.Generate(60);

            var rng = new Random();
            var size = Map.Size;

            GetGoals();
            var optimize = 3;
            if (CheckOptimize.Checked)
            {
                optimize = 10;
            }
            var wall = Walling.GenerateWall(Map, Goals, optimize);
            foreach (var point in wall)
            {
                Map.Tiles[point.X, point.Y].Type = AnalysisTileType.WALL;
            }

            var interior = FloodFill.GetInterior(Map.Center, p => Map.GetNeighbours(p), p => Map.Tiles[p.X, p.Y].Type == AnalysisTileType.NONE);
            LabelInterior.Text = $"Interior: {interior.Count}";
            LabelWallCount.Text = $"Wall length: {wall.Count}";

            Refresh();

            return wall;
        }

        private void WallingForm_MouseClick(object sender, MouseEventArgs e)
        {
            var x = e.X / TILE_SIZE;
            var y = e.Y / TILE_SIZE;

            Debug.WriteLine($"clicked {x} {y}");
        }

        private void GetGoals()
        {
            const int MIN_SIZE = 10;
            const int RESOURCE_CLEARANCE = 2;

            var size = Map.Size;
            var center = new Point(size / 2, size / 2);

            var half_size = (int)Math.Round(0.7d * MIN_SIZE);
            var goals = new List<Point>()
            {
                center,
                new Point(center.X + MIN_SIZE, center.Y),
                new Point(center.X + half_size, center.Y + half_size),
                new Point(center.X, center.Y + MIN_SIZE),
                new Point(center.X - half_size, center.Y + half_size),
                new Point(center.X - MIN_SIZE, center.Y),
                new Point(center.X - half_size, center.Y - half_size),
                new Point(center.X, center.Y - MIN_SIZE),
                new Point(center.X + half_size, center.Y - half_size)
            };

            var clumps = Map.GetResourceClumps();
            clumps.Sort((a, b) =>
            {
                return a.Min(p => AnalysisMap.WallDistance(p, center)).CompareTo(b.Min(p => AnalysisMap.WallDistance(p, center)));
            });

            var wood = false;
            var food = false;
            var gold = false;
            var stone = false;

            var positions = new List<Point>();
            foreach (var clump in clumps)
            {
                if (CheckAllResources.Checked)
                {
                    wood = false;
                    food = false;
                    gold = false;
                    stone = false;
                }

                positions.Clear();

                var pos = clump.First();
                var tile = Map.Tiles[pos.X, pos.Y];

                if (tile.IsResource)
                {
                    // TODO base method on clump size rather than resource type
                    if (tile.Type == AnalysisTileType.WOOD)
                    {
                        var add = !wood;

                        if (add)
                        {
                            wood = true;

                            positions.AddRange(clump);
                            positions.Sort((a, b) =>
                            {
                                var pa = Position.FromPoint(a.X - center.X, a.Y - center.Y);
                                var pb = Position.FromPoint(b.X - center.X, b.Y - center.Y);

                                var aa = pa.Angle;
                                var ab = pb.Angle;

                                if (pa.X < 0 && pb.X < 0 && aa < 0)
                                {
                                    aa += 2 * Math.PI;
                                }

                                if (pa.X < 0 && pb.X < 0 && ab < 0)
                                {
                                    ab += 2 * Math.PI;
                                }

                                return aa.CompareTo(ab);
                            });

                            goals.Add(positions[0]);
                            goals.Add(positions[positions.Count - 1]);
                        }
                    }
                    else
                    {
                        var add = false;
                        if (tile.Type == AnalysisTileType.FOOD && !food)
                        {
                            add = true;
                            food = true;
                        }
                        else if (tile.Type == AnalysisTileType.GOLD && !gold)
                        {
                            add = true;
                            gold = true;
                        }
                        else if (tile.Type == AnalysisTileType.STONE && !stone)
                        {
                            add = true;
                            stone = true;
                        }

                        if (add)
                        {
                            var xmin = int.MaxValue;
                            var xmax = int.MinValue;
                            var ymin = int.MaxValue;
                            var ymax = int.MinValue;

                            foreach (var point in clump)
                            {
                                xmin = Math.Min(xmin, point.X);
                                xmax = Math.Max(xmax, point.X);
                                ymin = Math.Min(ymin, point.Y);
                                ymax = Math.Max(ymax, point.Y);
                            }

                            xmin = Math.Max(0, xmin - RESOURCE_CLEARANCE);
                            xmax = Math.Min(size - 1, xmax + RESOURCE_CLEARANCE);
                            ymin = Math.Max(0, ymin - RESOURCE_CLEARANCE);
                            ymax = Math.Min(size - 1, ymax + RESOURCE_CLEARANCE);

                            goals.Add(new Point(xmin, ymin));
                            goals.Add(new Point(xmin, ymax));
                            goals.Add(new Point(xmax, ymin));
                            goals.Add(new Point(xmax, ymax));
                        }
                    }
                }

                
            }

            Goals.Clear();
            Goals.AddRange(goals);
        }
    }
}
