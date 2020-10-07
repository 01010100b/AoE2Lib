using AoE2Lib.Bots;
using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading;
using static AoE2Lib.Bots.Command;

namespace AoE2Lib
{
    public abstract class Bot
    {
        private const int SYNC_GOAL1 = 511;
        private const int SYNC_GOAL2 = 512;
        private const int FREE_SN = 350;

        public abstract string Name { get; }
        public abstract int Id { get; }
        public virtual string Author => "";
        public virtual string Email => "";
        public virtual string Url => "";

        public bool Running { get; private set; } = false;
        public int PlayerNumber { get; private set; } = -1;
        protected readonly Random RNG = new Random(Guid.NewGuid().GetHashCode() ^ DateTime.UtcNow.Ticks.GetHashCode());

        // game state
        protected Position MyPosition { get; private set; } = new Position(-1, -1);
        protected int MapWidthHeight { get; private set; } = -1;
        protected IReadOnlyList<Player> Players => _Players;
        private readonly List<Player> _Players = new List<Player>();
        protected IReadOnlyDictionary<Position, Tile> Tiles => _Tiles;
        private readonly Dictionary<Position, Tile> _Tiles = new Dictionary<Position, Tile>();
        protected IReadOnlyDictionary<int, Unit> Units => _Units;
        private readonly Dictionary<int, Unit> _Units = new Dictionary<int, Unit>();
        protected IReadOnlyList<UnitTypeInfo> UnitTypeInfos => _UnitTypeInfos;
        private readonly List<UnitTypeInfo> _UnitTypeInfos = new List<UnitTypeInfo>();

        // utils
        private Thread BotThread { get; set; } = null;
        private GameInstance Instance { get; set; } = null;
        private int[] Goals { get; set; } = null;
        private int[] StrategicNumbers { get; set; } = null;

        private volatile bool Stopping = false;

        public void Start(GameInstance instance, int player)
        {
            Stopping = true;
            BotThread?.Join();

            Instance = instance;
            PlayerNumber = player;

            // TODO reset game state

            Running = true;
            Stopping = false;

            BotThread = new Thread(() => Run());
            BotThread.IsBackground = true;
            BotThread.Start();
        }

        public void Stop()
        {
            Stopping = true;
        }

        protected abstract void StartGame();

        protected abstract Command Update(int tick);

        protected IEnumerable<Tile> GetTilesBydistance(Position position)
        {
            yield return Tiles[position];

            for (int r = 1; r < MapWidthHeight; r++)
            {
                int x, y;

                x = position.X - r;
                if (x >= 0)
                {
                    for (int dy = -r; dy <= r; dy++)
                    {
                        y = position.Y + dy;
                        var pos = new Position(x, y);

                        if (pos.OnMap(MapWidthHeight))
                        {
                            yield return Tiles[pos];
                        }
                    }
                }
                
                x = position.X + r;
                if (x < MapWidthHeight)
                {
                    for (int dy = -r; dy <= r; dy++)
                    {
                        y = position.Y + dy;
                        var pos = new Position(x, y);

                        if (pos.OnMap(MapWidthHeight))
                        {
                            yield return Tiles[pos];
                        }
                    }
                }
                
                y = position.Y - r;
                if (y >= 0)
                {
                    for (int dx = -r + 1; dx <= r - 1; dx++)
                    {
                        x = position.X + dx;
                        var pos = new Position(x, y);

                        if (pos.OnMap(MapWidthHeight))
                        {
                            yield return Tiles[pos];
                        }
                    }
                }
                
                y = position.Y + r;
                if (y < MapWidthHeight)
                {
                    for (int dx = -r + 1; dx <= r - 1; dx++)
                    {
                        x = position.X + dx;
                        var pos = new Position(x, y);

                        if (pos.OnMap(MapWidthHeight))
                        {
                            yield return Tiles[pos];
                        }
                    }
                }
            }
        }

        private void Run()
        {
            StartGame();

            var frametime = new Stopwatch();
            while (!Stopping)
            {
                frametime.Restart();

                if (!TryUpdate())
                {
                    break;
                }

                var sleep = 100 - (int)frametime.ElapsedMilliseconds;
                if (sleep > 1)
                {
                    Thread.Sleep(sleep);
                }
            }

            frametime.Stop();

            Running = false;
            Stopping = false;
            PlayerNumber = -1;
        }

        private bool TryUpdate()
        {
            var goals = Instance.GetGoals(PlayerNumber);

            if (goals == null)
            {
                return false;
            }

            var sns = Instance.GetStrategicNumbers(PlayerNumber);

            if (sns == null)
            {
                return false;
            }

            if (goals[SYNC_GOAL1 - 1] < 1)
            {
                return true;
            }

            if (goals[SYNC_GOAL1 - 1] != goals[SYNC_GOAL2 - 1])
            {
                return true;
            }

            if (Goals[SYNC_GOAL1 - 1] == goals[SYNC_GOAL1 - 1])
            {
                return true;
            }

            Goals = goals;
            StrategicNumbers = sns;

            UpdateGameState();
            var command = Update(Goals[SYNC_GOAL1 - 1]);
            GiveCommand(command);

            return true;
        }

        private void UpdateGameState()
        {
            throw new NotImplementedException();
        }

        private void GiveCommand(Command command)
        {
            CheckCommand(command);

            throw new NotImplementedException();
        }

        private void CheckCommand(Command command)
        {
            if (command.TilesToCheck.Count < 10)
            {
                var tile_time = DateTime.UtcNow - TimeSpan.FromMinutes(2);

                foreach (var tile in GetTilesBydistance(MyPosition).Where(t => t.LastUpdate < tile_time && !t.Explored))
                {
                    if (command.TilesToCheck.Count < 10)
                    {
                        command.TilesToCheck.Add(tile.Position);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (command.UnitSearch1Player < 0 || command.UnitSearch2Player < 0 || command.UnitSearch3Player < 0)
            {
                var explored = Tiles.Values.Where(t => t.Explored).Select(t => t.Position).ToList();
                if (explored.Count == 0)
                {
                    explored.Add(MyPosition);
                }

                var player = PlayerNumber;
                if (RNG.NextDouble() < 0.5)
                {
                    player = 0;

                    if (RNG.NextDouble() < 0.5)
                    {
                        player = RNG.Next(9);
                    }
                }
                
                var type = UnitSearchType.CIVILIAN;
                if (RNG.NextDouble() < 0.5)
                {
                    type = UnitSearchType.MILITARY;
                }

                if (player == 0)
                {
                    type = UnitSearchType.RESOURCE;

                    if (RNG.NextDouble() < 0.1)
                    {
                        type = UnitSearchType.ALL;
                    }
                }

                if (command.UnitSearch1Player < 0)
                {
                    command.SearchForUnits1(player, explored[RNG.Next(explored.Count)], 20, type);
                }

                if (command.UnitSearch2Player < 0)
                {
                    command.SearchForUnits2(player, explored[RNG.Next(explored.Count)], 20, type);
                }

                if (command.UnitSearch3Player < 0)
                {
                    command.SearchForUnits3(player, explored[RNG.Next(explored.Count)], 20, type);
                }
            }

            throw new NotImplementedException();
        }

        private int GetGoal(int id)
        {
            return Goals[id - 1];
        }

        private bool SetGoal(int id, int value)
        {
            return Instance.SetGoal(PlayerNumber, id - 1, value);
        }

        private bool SetGoals(int start_id, int[] values)
        {
            return Instance.SetGoals(PlayerNumber, start_id - 1, values);
        }

        private int GetStrategicNumber(int id)
        {
            return StrategicNumbers[id];
        }

        private bool SetStrategicNumber(int id, int value)
        {
            return Instance.SetStrategicNumber(PlayerNumber, id, value);
        }

        private bool SetStrategicNumbers(int start_id, params int[] values)
        {
            return Instance.SetStrategicNumbers(PlayerNumber, start_id, values);
        }
    }
}
