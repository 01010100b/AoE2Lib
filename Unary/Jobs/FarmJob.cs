using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Behaviours;
using Unary.Managers;

namespace Unary.Jobs
{
    internal class FarmJob : ResourceGenerationJob
    {
        private static readonly Point[] TC_FARM_DELTAS = { new Point(2, 3), new Point(-1, 3), new Point(3, 0), new Point(3, -3), new Point(-4, 2), new Point(-4, -1), new Point(0, -4), new Point(-3, -4) };
        private static readonly Point[] MILL_FARM_DELTAS = { new Point(-1, 2), new Point(2, -1), new Point(2, 2), new Point(-3, -1), new Point(-1, -3) };

        public override Resource Resource => Resource.FOOD;
        public override int MaxWorkers => Assignments.Count;
        public override string Name => $"Farming at {Location}";

        private readonly Dictionary<Tile, Controller> Assignments = new();

        public FarmJob(Unary unary, Controller dropsite) : base(unary, dropsite)
        {
        }

        public override double GetPay(Controller worker)
        {
            if (!worker.HasBehaviour<FarmingBehaviour>())
            {
                return -1;
            }
            else if (HasWorker(worker))
            {
                return GetPay();
            }
            else if (Vacancies < 1)
            {
                return -1;
            }
            else
            {
                return GetPay();
            }
        }

        protected override void OnWorkerJoining(Controller worker)
        {
        }

        protected override void OnWorkerLeaving(Controller worker)
        {
            if (worker.TryGetBehaviour<FarmingBehaviour>(out var behaviour))
            {
                behaviour.Tile = null;
            }
        }

        protected override void OnClosed()
        {
        }

        protected override void UpdateResourceGeneration()
        {
            Assignments.Clear();

            foreach (var tile in GetFarmTiles())
            {
                Assignments.Add(tile, null);
            }

            foreach (var worker in GetWorkers())
            {
                if (worker.TryGetBehaviour<FarmingBehaviour>(out var behaviour))
                {
                    if (behaviour.Tile != null && !Assignments.ContainsKey(behaviour.Tile))
                    {
                        behaviour.Tile = null;
                    }

                    if (behaviour.Tile == null)
                    {
                        foreach (var kvp in Assignments)
                        {
                            if (kvp.Value == null)
                            {
                                Assignments[kvp.Key] = worker;
                            }
                        }
                    }
                }
            }
        }

        private double GetPay()
        {
            var carry = 10;
            var rate = 0.4;
            var speed = 0.8;
            var distance = 4;

            return Utils.GetGatherRate(rate, 2 * (distance + 0.5), speed, carry);
        }

        private IEnumerable<Tile> GetFarmTiles()
        {
            var civ = Unary.Mod.GetCivInfo(Unary.GameState.MyPlayer.Civilization);
            var deltas = Dropsite.Unit[ObjectData.BASE_TYPE] == civ.TownCenterId ? TC_FARM_DELTAS : MILL_FARM_DELTAS;
            var pos = Dropsite.Unit.Position;

            foreach (var delta in deltas)
            {

            }
            throw new NotImplementedException();
        }

        private IEnumerable<Tile> GetFarmTilesNew()
        {
            var civ = Unary.Mod.GetCivInfo(Unary.GameState.MyPlayer.Civilization);

            if (Unary.GameState.TryGetUnitType(civ.FarmId, out var farm))
            {
                var size = Math.Max(civ.GetUnitWidth(civ.FarmId), civ.GetUnitHeight(civ.FarmId)) / 2;
                var dsize = Math.Max(civ.GetUnitWidth(Dropsite.Unit[ObjectData.BASE_TYPE]), civ.GetUnitHeight(Dropsite.Unit[ObjectData.BASE_TYPE]));
                var footprint = Utils.GetUnitFootprint(Dropsite.Unit.Position.PointX, Dropsite.Unit.Position.PointY, dsize, dsize);

                for (var x = footprint.Right + size; x >= footprint.Left - size; x--)
                {
                    var position = Position.FromPoint(x, footprint.Bottom + size);

                    if (Unary.GameState.Map.TryGetTile(position, out var tile))
                    {
                        yield return tile;
                    }
                }
            }

            throw new NotImplementedException();
        }
    }
}
