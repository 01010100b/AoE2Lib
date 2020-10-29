using Unary.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using Google.Protobuf;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using Google.Protobuf.WellKnownTypes;
using System.Runtime.InteropServices;

namespace Unary.GameElements
{
    public class Tile : GameElement
    {
        public readonly Position Position;
        public int Elevation { get; private set; } = -1;
        public int Terrain { get; private set; } = -1;
        public bool Explored { get; private set; } = false;

        public Tile(Position position) : base()
        {
            Position = position;
        }

        protected override void UpdateElement(List<Any> responses)
        {
            Elevation = responses[4].Unpack<GoalResult>().Result;
            Terrain = responses[6].Unpack<GoalResult>().Result;
            Explored = responses[7].Unpack<UpPointExploredResult>().Result;
        }

        protected override IEnumerable<IMessage> RequestElementUpdate()
        {
            var messages = new List<IMessage>()
            {
                new UpModifyGoal() {GoalId = 50, MathOp = 0, Value = Position.X},
                new UpModifyGoal() {GoalId = 51, MathOp = 0, Value = Position.Y},
                new UpBoundPoint() {GoalPoint1 = 52, GoalPoint2 = 50 },
                new UpGetPointElevation() {GoalPoint = 52, GoalData = 100},
                new Goal() {GoalId = 100},
                new UpGetPointTerrain() {GoalPoint = 52, GoalTerrain = 100},
                new Goal() {GoalId = 100},
                new UpPointExplored() {GoalPoint = 52},
            };

            return messages;
        }
    }
}
