using AoE2Lib.Utils;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AoE2Lib.Bots.GameElements
{
    public class Tile : GameElement
    {
        public readonly Point Point;
        public readonly int X;
        public readonly int Y;
        public Position Position => Position.FromPoint(Point.X, Point.Y);
        public bool Explored { get; private set; } = false;
        public int Elevation { get; private set; } = -1;
        public int Terrain { get; private set; } = -1;
        public bool IsOnLand => Terrain != 1 && Terrain != 2 && Terrain != 4 && Terrain != 15 && Terrain != 22 && Terrain != 23 && Terrain != 28 && Terrain != 37;
        public IEnumerable<Unit> Units => UnitsInternal;
        
        internal readonly List<Unit> UnitsInternal = new List<Unit>();

        internal Tile(Bot bot, Point point) : base(bot)
        {
            Point = point;
            X = point.X;
            Y = point.Y;
            Explored = false;
            Elevation = -1;
            Terrain = -1;
        }

        protected override IEnumerable<IMessage> RequestElementUpdate()
        {
            yield return new SetGoal() { InConstGoalId = 50, InConstValue = Point.X };
            yield return new SetGoal() { InConstGoalId = 51, InConstValue = Point.Y };
            yield return new UpBoundPoint() { OutGoalPoint = 52, InGoalPoint = 50 };
            yield return new UpPointElevation() { InGoalPoint = 52 };
            yield return new UpPointTerrain() { InGoalPoint = 52 };
            yield return new UpPointExplored() { InGoalPoint = 52 };
        }

        protected override void UpdateElement(IReadOnlyList<Any> responses)
        {
            Elevation = responses[3].Unpack<UpPointElevationResult>().Result;
            Terrain = responses[4].Unpack<UpPointTerrainResult>().Result;
            Explored = responses[5].Unpack<UpPointExploredResult>().Result != 0;
        }
    }
}
