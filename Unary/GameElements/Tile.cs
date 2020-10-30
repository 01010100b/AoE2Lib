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
            Elevation = responses[3].Unpack<UpPointElevationResult>().Result;
            Terrain = responses[4].Unpack<UpPointTerrainResult>().Result;
            Explored = responses[5].Unpack<UpPointExploredResult>().Result != 0;
        }

        protected override IEnumerable<IMessage> RequestElementUpdate()
        {
            var messages = new List<IMessage>()
            {
                new SetGoal() {GoalId = 50, GoalValue = Position.X},
                new SetGoal() {GoalId = 51, GoalValue = Position.Y},
                new UpBoundPoint() {GoalPoint1 = 52, GoalPoint2 = 50 },
                new UpPointElevation() {GoalPoint = 52},
                new UpPointTerrain() {GoalPoint = 52},
                new UpPointExplored() {GoalPoint = 52},
            };

            return messages;
        }
    }
}
