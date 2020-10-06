using AoE2Lib.Bots;
using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace AoE2Lib
{
    public abstract class Bot
    {
        protected enum UnitSearchType
        {
            MILITARY, CIVILIAN, WOOD, FOOD, GOLD, STONE
        }

        private const int SYNC_GOAL1 = 511;
        private const int SYNC_GOAL2 = 512;

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
        protected IReadOnlyDictionary<int, Player> Players => _Players;
        private readonly Dictionary<int, Player> _Players = new Dictionary<int, Player>();
        protected IReadOnlyDictionary<Position, Tile> Map => _Map;
        private readonly Dictionary<Position, Tile> _Map = new Dictionary<Position, Tile>();
        protected IReadOnlyDictionary<int, Unit> Units => _Units;
        private readonly Dictionary<int, Unit> _Units = new Dictionary<int, Unit>();

        // commands
        private readonly List<Position> TilesToCheck = new List<Position>();
        private int UnitSearch1Player { get; set; } = -1;
        private Position UnitSearch1Position { get; set; } = new Position(-1, -1);
        private int UnitSearch1Radius { get; set; } = -1;
        private UnitSearchType UnitSearch1Type { get; set; } = UnitSearchType.CIVILIAN;
        private int UnitSearch2Player { get; set; } = -1;
        private Position UnitSearch2Position { get; set; } = new Position(-1, -1);
        private int UnitSearch2Radius { get; set; } = -1;
        private UnitSearchType UnitSearch2Type { get; set; } = UnitSearchType.CIVILIAN;

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

        protected abstract void Update(int tick);

        protected void CheckTile(Position position)
        {
            TilesToCheck.Add(position);
        }

        protected void SearchForUnits1(int player, Position position, int radius, UnitSearchType type)
        {
            UnitSearch1Player = player;
            UnitSearch1Position = position;
            UnitSearch1Radius = radius;
            UnitSearch1Type = type;
        }

        protected void SearchForUnits2(int player, Position position, int radius, UnitSearchType type)
        {
            UnitSearch2Player = player;
            UnitSearch2Position = position;
            UnitSearch2Radius = radius;
            UnitSearch2Type = type;
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
            Update(Goals[SYNC_GOAL1 - 1]);

            return true;
        }

        private void UpdateGameState()
        {

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
