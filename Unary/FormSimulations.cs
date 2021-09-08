using AoE2Lib.Bots;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Unary.Simulations;
using static Unary.Simulations.BattleSimulation;

namespace Unary
{
    public partial class FormSimulations : Form
    {
        const int SIM_SIZE = 20;

        private BattleSimulation BattleSimulation { get; set; }
        private readonly Random Rng = new Random();

        public FormSimulations()
        {
            InitializeComponent();

            CreateSimulation();
        }

        private void CreateSimulation()
        {
            BattleSimulation = new BattleSimulation(p => 0, p => p.X < 0 || p.X > SIM_SIZE || p.Y < 0 || p.Y > SIM_SIZE, (u, t) => 4);

            var red_policy = new BasicPolicy() { FocusFire = true, NoOverkill = false, Avoid = true };
            var blue_policy = new BasicPolicy() { FocusFire = true, NoOverkill = false, Avoid = false };
            BattleSimulation.SetPolicy(0, red_policy);
            BattleSimulation.SetPolicy(1, blue_policy);
            
            for (int i = 0; i < 10; i++)
            {
                var unit = CreateUnit(0);
                BattleSimulation.AddUnit(unit);
            }

            for (int i = 0; i < 10; i++)
            {
                var unit = CreateUnit(1);
                BattleSimulation.AddUnit(unit);
            }
        }

        private BattleUnit CreateUnit(int player)
        {
            var pos = new Position(Rng.NextDouble() * SIM_SIZE / 3, Rng.NextDouble() * SIM_SIZE / 3);
            if (player == 1)
            {
                pos = new Position(SIM_SIZE, SIM_SIZE) - pos;
            }

            var unit = new BattleUnit(player, 30, 0.3, 0.96, 7, TimeSpan.FromSeconds(2), 4, pos);

            return unit;
        }

        private void Tick()
        {
            if (BattleSimulation == null)
            {
                return;
            }

            BattleSimulation.Tick(TimeSpan.FromMilliseconds((double)NumericTicktime.Value));
        }

        private void FormSimulations_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var scale = 0.99f * Math.Min(g.VisibleClipBounds.Width, g.VisibleClipBounds.Height) / (float)SIM_SIZE;
            g.DrawRectangle(Pens.Black, 0, 0, scale * SIM_SIZE, scale * SIM_SIZE);
            var brush0 = new SolidBrush(Color.Red);
            var brush1 = new SolidBrush(Color.Blue);
            var pen0 = Pens.Red;
            var pen1 = Pens.Blue;

            foreach (var unit in BattleSimulation.GetUnits())
            {
                var x = unit.CurrentPosition.X - unit.Radius;
                var y = unit.CurrentPosition.Y - unit.Radius;
                var size = 2 * unit.Radius;

                var brush = brush0;
                if (unit.Player == 1)
                {
                    brush = brush1;
                }

                g.FillEllipse(brush, scale * (float)x, scale * (float)y, scale * (float)size, scale * (float)size);

                if (unit.HasProjectileInFlight)
                {
                    var to = unit.ProjectileTargetPosition - unit.ProjectilePosition;
                    to /= 2 * to.Norm;
                    to += unit.ProjectilePosition;

                    var pen = pen0;
                    if (unit.Player == 1)
                    {
                        pen = pen1;
                    }

                    var x1 = (float)unit.ProjectilePosition.X;
                    var y1 = (float)unit.ProjectilePosition.Y;
                    var x2 = (float)to.X;
                    var y2 = (float)to.Y;

                    g.DrawLine(pen, scale * x1, scale * y1, scale * x2, scale * y2);
                }
            }

            LabelGameTime.Text = $"Game time: {BattleSimulation.GameTime:g}";

            if (BattleSimulation.GetUnits(0).Count() == 0)
            {
                LabelGameTime.Text += " BLUE WINS";
            }
            else if (BattleSimulation.GetUnits(1).Count() == 0)
            {
                LabelGameTime.Text += " RED WINS";
            }
        }

        private void ButtonTick_Click(object sender, EventArgs e)
        {
            if (BattleSimulation.GetUnits(0).Count() == 0 || BattleSimulation.GetUnits(1).Count() == 0)
            {
                CreateSimulation();
            }

            TimerTick.Enabled = !TimerTick.Enabled;
        }

        private void TimerTick_Tick(object sender, EventArgs e)
        {
            Invoke(new Action(() =>
            {
                Tick();
                Refresh();

                if (BattleSimulation == null)
                {
                    return;
                }

                if (BattleSimulation.GetUnits(0).Count() == 0)
                {
                    TimerTick.Enabled = false;
                }
                else if (BattleSimulation.GetUnits(1).Count() == 0)
                {
                    TimerTick.Enabled = false;
                }
            }));
        }

        private void ButtonTest_Click(object sender, EventArgs e)
        {
            ButtonTest.Enabled = false;
            TimerTick.Enabled = false;
            Cursor = Cursors.WaitCursor;

            var gametime = TimeSpan.Zero;
            var sw = new Stopwatch();
            sw.Start();

            var total = 0;
            var red = 0d;
            while (sw.Elapsed.TotalSeconds < 10)
            {
                if (BattleSimulation.GetUnits(0).Count() == 0 || BattleSimulation.GetUnits(1).Count() == 0)
                {
                    gametime += BattleSimulation.GameTime;
                    total++;
                    if (BattleSimulation.GetUnits(1).Count() == 0)
                    {
                        red++;
                    }

                    CreateSimulation();
                }

                Tick();
            }

            gametime += BattleSimulation.GameTime;
            sw.Stop();

            var lines = new[] { $"Red won {red / total:P}", $"Played {total:N0} games", $"Speedup {gametime.TotalSeconds / sw.Elapsed.TotalSeconds:N2}" };
            TextOutput.Lines = lines;

            Refresh();

            Cursor = Cursors.Default;
            ButtonTest.Enabled = true;
        }

        private void NumericTicktime_ValueChanged(object sender, EventArgs e)
        {
            TimerTick.Interval = (int)NumericTicktime.Value;
        }
    }
}
