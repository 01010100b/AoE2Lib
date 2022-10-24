using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Jobs;
using Unary.Managers;

namespace Unary.Behaviours
{
    internal class Controller
    {
        public readonly Unit Unit;
        public readonly Unary Unary;
        public bool Exists => Unit.Targetable;
        public Job CurrentJob { get; internal set; }

        private readonly List<Behaviour> Behaviours = new();

        internal Controller(Unit unit, Unary unary)
        {
            Unit = unit;
            Unary = unary;

            AddDefaultBehaviours();
        }

        public void AddBehaviour<T>(T behaviour) where T : Behaviour
        {
            if (TryGetBehaviour<T>(out _))
            {
                return;
            }

            behaviour.Controller = this;
            Behaviours.Add(behaviour);
        }

        public bool HasBehaviour<T>() where T : Behaviour => TryGetBehaviour<T>(out _);

        public bool TryGetBehaviour<T>(out T behaviour) where T : Behaviour
        {
            foreach (var b in Behaviours.OfType<T>())
            {
                behaviour = b;

                return true;
            }

            behaviour = default;

            return false;
        }

        internal void Tick(Dictionary<Type, KeyValuePair<int, TimeSpan>> times)
        {
            if (Unary.ShouldRareTick(this, 31))
            {
                Unit.RequestUpdate();
            }

            Behaviours.Sort((a, b) => b.GetPriority().CompareTo(a.GetPriority()));

            var perform = true;
            var sw = new Stopwatch();

            foreach (var behaviour in Behaviours)
            {
                sw.Restart();

                if (behaviour.TickInternal(perform))
                {
                    perform = false;

                    Unary.Log.Debug($"Unit {Unit.Id} performed {behaviour.GetType().Name}");
                }

                sw.Stop();
                var type = behaviour.GetType();

                if (!times.ContainsKey(type))
                {
                    times.Add(type, new KeyValuePair<int, TimeSpan>(0, TimeSpan.Zero));
                }

                var time = times[type];
                times[type] = new KeyValuePair<int, TimeSpan>(time.Key + 1, time.Value + sw.Elapsed);
            }
        }

        private void AddDefaultBehaviours()
        {
            if (Unit[ObjectData.CMDID] == (int)CmdId.VILLAGER)
            {
                AddBehaviour(new JobBehaviour());
                AddBehaviour(new BuildBehaviour());
                AddBehaviour(new EatBehaviour());
                AddBehaviour(new GatherBehaviour());
            }
            else if (Unit[ObjectData.CMDID] == (int)CmdId.MILITARY)
            {
                AddBehaviour(new JobBehaviour());
                AddBehaviour(new CombatBehaviour());
            }
        }
    }
}
