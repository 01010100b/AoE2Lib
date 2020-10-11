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
using static AoE2Lib.Bots.UnitTypeInfo;

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
        
        protected readonly Random RNG = new Random(Guid.NewGuid().GetHashCode() ^ DateTime.UtcNow.Ticks.GetHashCode());
        protected GameState GameState { get; private set; } = null;
        protected Mod Mod { get; private set; } = null;
        protected int Tick { get; private set; } = 0;

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

            Debug.WriteLine("bot stopped");
        }

        private bool TryUpdate()
        {
            const int SYNC_GOAL1 = 512;
            const int SYNC_GOAL2 = 1;

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

            if (Goals != null && Goals[SYNC_GOAL1 - 1] == goals[SYNC_GOAL1 - 1])
            {
                return true;
            }

            Goals = goals;
            StrategicNumbers = sns;

            Tick = Goals[SYNC_GOAL1 - 1];

            UpdateGameState();
            var command = GetNextCommand();
            GiveCommand(command);

            return true;
        }

        private void UpdateGameState()
        {
            UpdateInfo();
            UpdatePlayers();
            UpdateTiles();
            UpdateUnits();
        }

        private void UpdateInfo()
        {
            const int GL_GAMETIME = 11;
            const int GL_MAPSIZE = 12;

            const int GL_WOOD = 21;
            const int GL_FOOD = 22;
            const int GL_GOLD = 23;
            const int GL_STONE = 24;
            const int GL_POPULATION_HEADROOM = 25;
            const int GL_HOUSING_HEADROOM = 26;
            const int GL_X = 27;
            const int GL_Y = 28;

            GameState.GameTime = TimeSpan.FromSeconds(GetGoal(GL_GAMETIME));
            GameState.MapWidthHeight = GetGoal(GL_MAPSIZE);
            
            GameState.WoodAmount = GetGoal(GL_WOOD);
            GameState.FoodAmount = GetGoal(GL_FOOD);
            GameState.GoldAmount = GetGoal(GL_GOLD);
            GameState.StoneAmount = GetGoal(GL_STONE);
            GameState.PopulationHeadroom = GetGoal(GL_POPULATION_HEADROOM);
            GameState.HousingHeadroom = GetGoal(GL_HOUSING_HEADROOM);
            GameState.MyPosition = new Position(GetGoal(GL_X), GetGoal(GL_Y));
        }

        private void UpdatePlayers()
        {
            const int GL_PLAYER_GOAL0 = 41;
            const int GL_PLAYER_GOAL1 = 42;

            var goal0 = GetGoal(GL_PLAYER_GOAL0);
            var goal1 = GetGoal(GL_PLAYER_GOAL1);

            if (goal0 >= 0)
            {
                var player = goal0 % 10;

                if (!GameState.Players.ContainsKey(player))
                {
                    GameState._Players.Add(player, new Player(player));
                }

                GameState.Players[player].Update(goal0, goal1);
            }

            if (!GameState.Players.ContainsKey(0))
            {
                GameState._Players.Add(0, new Player(0));
            }
        }

        private void UpdateTiles()
        {
            const int GL_TILES_START = 51;
            const int GL_TILES_END = 90;

            var offset = GL_TILES_START;
            while (offset <= GL_TILES_END)
            {
                var goal0 = GetGoal(offset);
                offset++;
                var goal1 = GetGoal(offset);
                offset++;

                if (goal0 >= 0)
                {
                    var x = goal0 / 500;
                    var y = goal0 % 500;
                    var position = new Position(x, y);

                    GameState.Tiles[position].Update(goal0, goal1);
                }
            }
        }

        private void UpdateUnits()
        {
            const int GL_UNITS_START = 151;
            const int GL_UNITS_END = 390;

            var offset = GL_UNITS_START;
            while (offset <= GL_UNITS_END)
            {
                var goal0 = GetGoal(offset);
                offset++;
                var goal1 = GetGoal(offset);
                offset++;
                var goal2 = GetGoal(offset);
                offset++;

                if (goal0 >= 0)
                {
                    var id = goal0 % 45000;

                    if (!GameState.Units.ContainsKey(id))
                    {
                        GameState._Units.Add(id, new Unit(id));
                    }

                    var unit = GameState.Units[id];
                    unit.Update(goal0, goal1, goal2);

                    var key = new UnitTypeInfoKey(unit.PlayerNumber, unit.TypeId);
                    if (!GameState.UnitTypeInfos.ContainsKey(key))
                    {
                        GameState._UnitTypeInfos.Add(key, new UnitTypeInfo(key));
                    }
                }
            }
        }

        private void GiveCommand(Command command)
        {
            const int SN_RANDOM = 350;

            const int SN_TILE_START = 351;
            const int SN_TILE_END = 370;

            const int SN_UNITSEARCH_START = 401;
            const int SN_UNITSEARCH_END = 404;

            const int SN_UNITTYPEINFO = 411;

            const int SN_TRAINING_START = 421;
            const int SN_TRAINING_END = 429;

            const int SN_BUILDING_START = 430;
            const int SN_BUILDING_END = 430;

            SetStrategicNumber(SN_RANDOM, RNG.Next(10000, 30000));

            // tiles

            var offset = SN_TILE_START;
            int sn;
            foreach (var pos in command.TilesToCheck)
            {
                sn = pos.X;
                sn *= 500;
                sn += pos.Y;

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
            foreach (var search in command.UnitSearchCommands)
            {
                sn = search.Player;
                sn *= 500;
                sn += search.Position.X;
                sn *= 500;
                sn += search.Position.Y;
                sn *= 100;
                sn += Math.Max(1, Math.Min(99, search.Radius));
                sn *= 8;
                sn += (int)search.SearchType;

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

            if (command.UnitTypeInfoPlayer >= 0 && command.UnitTypeInfoPlayer <= 8)
            {
                sn = command.UnitTypeInfoPlayer;
                sn *= 2000;
                sn += command.UnitTypeInfoType;
            }
            else
            {
                sn = -1;
            }

            SetStrategicNumber(SN_UNITTYPEINFO, sn);

            // training

            offset = SN_TRAINING_START;
            foreach (var goal in command.Training)
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
            foreach (var goal in command.Building)
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
