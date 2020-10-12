using AoE2Lib.Bots;
using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading;

namespace AoE2Lib
{
    public abstract class Bot
    {
        public abstract string Name { get; }
        public abstract int Id { get; }
        public virtual string Author => "";
        public virtual string Email => "";
        public virtual string Url => "";

        public bool Running { get; private set; } = false;
        public int PlayerNumber { get; private set; } = -1;

        protected GameState GameState { get; private set; } = null;
        protected Mod Mod { get; private set; } = null;
        protected int Tick { get; private set; } = 0;
        protected readonly Random RNG = new Random(Guid.NewGuid().GetHashCode() ^ DateTime.UtcNow.Ticks.GetHashCode());

        private Thread BotThread { get; set; } = null;
        private GameInstance Instance { get; set; } = null;
        private int[] Goals { get; set; } = null;
        private int[] StrategicNumbers { get; set; } = null;
        
        private volatile bool Stopping = false;

        public void Start(GameInstance instance, int player, Mod mod)
        {
            Stopping = true;
            BotThread?.Join();

            Instance = instance;
            PlayerNumber = player;
            GameState = new GameState();
            Mod = mod;
            Tick = 0;

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

        protected abstract Command GetNextCommand();

        protected int GetStrategicNumber(StrategicNumber number)
        {
            return GetStrategicNumber((int)number);
        }

        protected bool SetStrategicNumber(StrategicNumber number, int value)
        {
            return SetStrategicNumber((int)number, value);
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
            GameState = null;
            Mod = null;
            Tick = 0;

            Debug.WriteLine("bot stopped");
        }

        private bool TryUpdate()
        {
            const int SYNC_GOAL1 = 512;
            const int SYNC_GOAL2 = 1;

            var goals = Instance.GetGoals(PlayerNumber);

            if (goals == null)
            {
                return true;
            }

            var sns = Instance.GetStrategicNumbers(PlayerNumber);

            if (sns == null)
            {
                return true;
            }

            if (goals[SYNC_GOAL1 - 1] < 1)
            {
                return true;
            }

            if (goals[SYNC_GOAL1 - 1] != goals[SYNC_GOAL2 - 1])
            {
                return true;
            }

            if (Goals != null && Goals[SYNC_GOAL1 - 1] == goals[SYNC_GOAL1 - 1])
            {
                return true;
            }

            Goals = goals;
            StrategicNumbers = sns;

            Tick = Goals[SYNC_GOAL1 - 1];

            GameState.Update(Goals);
            var command = GetNextCommand();
            GiveCommand(command);

            return true;
        }
        
        private void GiveCommand(Command command)
        {
            const int SN_RANDOM = 350;

            const int SN_TILE_START = 351;
            const int SN_TILE_END = 370;

            const int SN_UNITSEARCH_START = 401;
            const int SN_UNITSEARCH_END = 404;

            const int SN_UNITTYPEINFO_START = 411;
            const int SN_UNITTYPEINFO_END = 411;

            const int SN_TRAINING_START = 421;
            const int SN_TRAINING_END = 429;

            const int SN_BUILDING_START = 430;
            const int SN_BUILDING_END = 430;

            SetStrategicNumber(SN_RANDOM, RNG.Next());

            // tiles

            var offset = SN_TILE_START;
            
            foreach (var sn in command.CheckTileCommands)
            {
                SetStrategicNumber(offset, sn);
                offset++;

                if (offset > SN_TILE_END)
                {
                    break;
                }
            }

            while (offset <= SN_TILE_END)
            {
                SetStrategicNumber(offset, -1);
                offset++;
            }

            // unit search

            offset = SN_UNITSEARCH_START;
            foreach (var sn in command.UnitSearchCommands)
            {
                SetStrategicNumber(offset, sn);
                offset++;

                if (offset > SN_UNITSEARCH_END)
                {
                    break;
                }
            }

            while (offset <= SN_UNITSEARCH_END)
            {
                SetStrategicNumber(offset, -1);
                offset++;
            }

            // unit type info

            offset = SN_UNITTYPEINFO_START;
            foreach (var sn in command.UnitTypeInfoCommands)
            {
                SetStrategicNumber(offset, sn);
                offset++;

                if (offset > SN_UNITTYPEINFO_END)
                {
                    break;
                }
            }

            while (offset <= SN_UNITTYPEINFO_END)
            {
                SetStrategicNumber(offset, -1);
                offset++;
            }

            // training

            offset = SN_TRAINING_START;
            foreach (var goal in command.TrainCommands)
            {
                SetStrategicNumber(offset, goal);
                offset++;

                if (offset > SN_TRAINING_END)
                {
                    break;
                }
            }

            while (offset <= SN_TRAINING_END)
            {
                SetStrategicNumber(offset, -1);
                offset++;
            }

            // building

            offset = SN_BUILDING_START;
            foreach (var goal in command.BuildCommands)
            {
                SetStrategicNumber(offset, goal);
                offset++;

                if (offset > SN_BUILDING_END)
                {
                    break;
                }
            }

            while (offset <= SN_BUILDING_END)
            {
                SetStrategicNumber(offset, -1);
                offset++;
            }
        }

        private int GetGoal(int id)
        {
            return Goals[id - 1];
        }

        private bool SetGoal(int id, int value)
        {
            if (GetGoal(id) != value)
            {
                return Instance.SetGoal(PlayerNumber, id - 1, value);
            }

            return true;
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
            if (GetStrategicNumber(id) != value)
            {
                return Instance.SetStrategicNumber(PlayerNumber, id, value);
            }

            return true;
        }

        private bool SetStrategicNumbers(int start_id, params int[] values)
        {
            return Instance.SetStrategicNumbers(PlayerNumber, start_id, values);
        }
    }
}
