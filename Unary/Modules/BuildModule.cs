using Protos.Expert.Action;
using Protos.Expert.Fact;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.Utils;

namespace Unary.Modules
{
    public class BuildModule : Module
    {
        private int BuildingId { get; set; } = -1;
        private int Size { get; set; } = -1;
        private Position Position { get; set; } = new Position(-1, -1);
        private int CurrentRadius { get; set; } = -1;
        private Position PlacePosition { get; set; } = new Position(-1, -1);
        private readonly Command Command = new Command();

        public void Build(int id, int size, Position position)
        {
            if (BuildingId != -1)
            {
                return;
            }

            BuildingId = id;
            Size = size;
            Position = position;
            CurrentRadius = 0;
        }
        
        internal override IEnumerable<Command> RequestUpdate(Bot bot)
        {
            Command.Messages.Clear();
            Command.Responses.Clear();

            if (BuildingId != -1)
            {
                if (bot.GameState.Tiles.ContainsKey(PlacePosition))
                {
                    Command.Messages.Add(new SetGoal() { GoalId = 100, GoalValue = PlacePosition.X });
                    Command.Messages.Add(new SetGoal() { GoalId = 101, GoalValue = PlacePosition.Y });
                    Command.Messages.Add(new UpBuildLine() { TypeOp = (int)TypeOp.C, BuildingId = BuildingId, GoalPoint1 = 100, GoalPoint2 = 100 });
                }
                else
                {
                    var tiles = bot.GameState.GetTilesInRange(Position, CurrentRadius);
                    foreach (var tile in tiles.Where(t => CanBuildAtPosition(bot.GameState, t.Position, Size)))
                    {
                        Command.Messages.Add(new SetGoal() { GoalId = 100, GoalValue = tile.Position.X });
                        Command.Messages.Add(new SetGoal() { GoalId = 101, GoalValue = tile.Position.Y });
                        Command.Messages.Add(new UpCanBuildLine() { TypeOp = (int)TypeOp.C, BuildingId = BuildingId, EscrowState = 0, GoalPoint = 100 });
                    }
                }
            }
            throw new NotImplementedException();
        }

        internal override void Update(Bot bot)
        {
            Debug.Assert(Command.Responses.Count == Command.Messages.Count);

            if (BuildingId != -1)
            {
                if (bot.GameState.Tiles.ContainsKey(PlacePosition))
                {
                    BuildingId = -1;
                    CurrentRadius = -1;
                    PlacePosition = new Position(-1, -1);
                }
                else
                {

                }
            }

            Command.Messages.Clear();
            Command.Responses.Clear();
            throw new NotImplementedException();
        }

        private bool CanBuildAtPosition(GameState state, Position position, int size)
        {
            if (!state.Tiles.ContainsKey(position))
            {
                return false;
            }

            var elevation = int.MinValue;

            for (int x = position.X; x <= position.X + size; x++)
            {
                for (int y = position.Y; y <= position.Y + size; y++)
                {
                    var pos = new Position(x, y);

                    if (!state.Tiles.ContainsKey(pos))
                    {
                        return false;
                    }

                    var tile = state.Tiles[pos];

                    if (!tile.Explored)
                    {
                        return false;
                    }

                    if (elevation == int.MinValue)
                    {
                        elevation = tile.Elevation;
                    }

                    if (tile.Elevation != elevation)
                    {
                        return false;
                    }

                    if (!IsTerrainBuildable(tile.Terrain))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool IsTerrainBuildable(int terrain)
        {
            throw new NotImplementedException();
        }
    }
}
