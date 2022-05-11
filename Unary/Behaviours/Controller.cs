﻿using AoE2Lib;
using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Managers;

namespace Unary.Behaviours
{
    internal class Controller
    {
        public readonly Unit Unit;
        public readonly Unary Unary;
        public UnitsManager Manager => Unary.UnitsManager;
        public bool Exists => Unit.Targetable;

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

        public T GetBehaviour<T>() where T : Behaviour
        {
            return Behaviours.OfType<T>().Cast<T>().FirstOrDefault();
        }

        public bool TryGetBehaviour<T>(out T behaviour) where T : Behaviour
        {
            var b = GetBehaviour<T>();

            if (b != default)
            {
                behaviour = b;

                return true;
            }
            else
            {
                behaviour = default;

                return false;
            }
        }

        internal void Tick(Dictionary<Type, KeyValuePair<int, TimeSpan>> times)
        {
            if (Unary.ShouldRareTick(this, 31))
            {
                Unit.RequestUpdate();
            }

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
            if (Unit.IsBuilding)
            {
                AddBehaviour(new ConstructionSpotBehaviour());

                if (Unit[ObjectData.BASE_TYPE] == Unary.Mod.TownCenter)
                {
                    AddBehaviour(new EatingSpotBehaviour());
                }
            }
            else
            {
                AddBehaviour(new FightAnimalBehaviour());

                if (Unit[ObjectData.CMDID] == (int)CmdId.VILLAGER)
                {
                    AddBehaviour(new BuildBehaviour());
                    AddBehaviour(new EatBehaviour());
                }
                else
                {
                    if (Unit[ObjectData.RANGE] > 2)
                    {
                        AddBehaviour(new CombatRangedBehaviour());
                    }
                }
            }
        }
    }
}
