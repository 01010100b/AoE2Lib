using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Behaviours
{
    internal class Controller
    {
        public readonly Unit Unit;
        public readonly Unary Unary;
        
        private readonly List<Behaviour> Behaviours = new();

        internal Controller(Unit unit, Unary unary)
        {
            Unit = unit;
            Unary = unary;

            AddDefaultBehaviours();
        }

        public void AddBehaviour<T>(T behaviour, params Type[] before) where T : Behaviour
        {
            if (GetBehaviour<T>() != null)
            {
                return;
            }

            var index = Behaviours.Count;

            for (int i = 0; i < Behaviours.Count; i++)
            {
                if (before.Contains(Behaviours[i].GetType()))
                {
                    index = Math.Min(index, i);
                }
            }

            Behaviours.Insert(index, behaviour);
        }

        public T GetBehaviour<T>() where T : Behaviour
        {
            return Behaviours.OfType<T>().Cast<T>().FirstOrDefault();
        }

        public void Tick()
        {
            if (GetHashCode() % 13 == Unary.GameState.Tick % 13)
            {
                Unit.RequestUpdate();
            }

            foreach (var behaviour in Behaviours.ToList())
            {
                if (behaviour.PerformInternal(this))
                {
                    return;
                }
            }
        }

        private void AddDefaultBehaviours()
        {
            AddBehaviour(new FightAnimalBehaviour());
        }
    }
}
