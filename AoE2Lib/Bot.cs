using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace AoE2Lib
{
    public abstract class Bot
    {
        private const int SYNC_GOAL1 = 511;
        private const int SYNC_GOAL2 = 512;

        public bool Running { get; private set; } = false;
        public int PlayerNumber { get; private set; } = -1;

        protected readonly Random RNG = new Random(Guid.NewGuid().GetHashCode() ^ DateTime.UtcNow.Ticks.GetHashCode());

        private GameInstance Instance { get; set; } = null;
        private int[] Goals { get; set; } = null;
        private int[] StrategicNumbers { get; set; } = null;

        private volatile bool Stopping = false;

        public void Start(GameInstance instance, int player)
        {

        }

        public void Stop()
        {

        }

        protected abstract void StartGame();

        protected abstract void Update(int tick);

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
                if (sleep > 10)
                {
                    Thread.Sleep(sleep);
                }
            }

            frametime.Stop();

            PlayerNumber = 0;
            Running = false;
        }

        private bool TryUpdate()
        {
            var goals = Instance.GetGoals(PlayerNumber);

            if (goals == null)
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

            var sns = Instance.GetStrategicNumbers(PlayerNumber);

            if (sns == null)
            {
                return false;
            }

            Goals = goals;
            StrategicNumbers = sns;
            

            Update(Goals[SYNC_GOAL1 - 1]);

            return true;
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
