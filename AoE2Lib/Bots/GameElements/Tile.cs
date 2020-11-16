using AoE2Lib.Utils;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots.GameElements
{
    public class Tile : GameElement
    {
        public readonly int X;
        public readonly int Y;
        public Vector2 Position => Vector2.FromPoint(X, Y);
        public int Elevation { get; private set; } = -1;
        public int Terrain { get; private set; } = -1;
        public bool Explored { get; private set; } = false;

        protected internal Tile(Bot bot, int x, int y) : base(bot)
        {
            X = x;
            Y = y;
        }

        protected override IEnumerable<IMessage> RequestElementUpdate()
        {
            yield return new SetGoal() { GoalId = 50, GoalValue = X };
            yield return new SetGoal() { GoalId = 51, GoalValue = Y };
            yield return new UpBoundPoint() { GoalPoint1 = 52, GoalPoint2 = 50 };
            yield return new UpPointElevation() { GoalPoint = 52 };
            yield return new UpPointTerrain() { GoalPoint = 52 };
            yield return new UpPointExplored() { GoalPoint = 52 };
        }

        protected override void UpdateElement(IReadOnlyList<Any> responses)
        {
            Explored = responses[5].Unpack<UpPointExploredResult>().Result != 0;

            if (Explored)
            {
                Elevation = responses[3].Unpack<UpPointElevationResult>().Result;
                Terrain = responses[4].Unpack<UpPointTerrainResult>().Result;
            }
        }
    }
}
