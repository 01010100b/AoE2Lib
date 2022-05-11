using MessagePack;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AoE2Lib.Games
{
    public class Game
    {
        // for an explanation for these values, see the aoc-auto-game repo: https://github.com/FLWL/aoc-auto-game

        public GameType GameType { get; set; } = GameType.RANDOM_MAP;
        public string ScenarioName { get; set; } = "";
        public MapType MapType { get; set; } = MapType.ARABIA;
        public MapSize MapSize { get; set; } = MapSize.TINY;
        public Difficulty Difficulty { get; set; } = Difficulty.HARD;
        public StartingResources StartingResources { get; set; } = StartingResources.STANDARD;
        public int PopulationLimit { get; set; } = 200;
        public RevealMap RevealMap { get; set; } = RevealMap.NORMAL;
        public StartingAge StartingAge { get; set; } = StartingAge.STANDARD;
        public VictoryType VictoryType { get; set; } = VictoryType.STANDARD;
        public int VictoryValue { get; set; } = 0; // for time limit, every 2 of value = 1 second
        public bool TeamsTogether { get; set; } = true;
        public bool LockTeams { get; set; } = false;
        public bool AllTechs { get; set; } = false;
        public bool Recorded { get; set; } = true;
        public DateTime LastProgressTimeUtc { get; private set; } = DateTime.MinValue;
        public bool Finished { get; private set; } = false;
        public IEnumerable<Player> Winners => GetWinners();

        private readonly List<Player> Players = new();
        private TcpClient Client { get; set; }
        private int NextId { get; set; }
        private Thread Thread { get; set; }
        private volatile bool Stopping = false;

        public void AddPlayer(Player player)
        {
            Players.Add(player);
        }

        public IEnumerable<Player> GetPlayers()
        {
            return Players;
        }

        public void Stop()
        {
            Stopping = true;
            Thread?.Join();
            Thread = null;

            if (Client != null)
            {
                Client.Close();
                Client = null;
            }

            Stopping = false;
        }

        internal void Start(IPEndPoint endpoint, bool minimized)
        {
            Stop();

            Finished = false;
            Client = new();
            NextId = 1;

            Client.Connect(endpoint);
            Setup(minimized);
            Call("StartGame");
            Thread.Sleep(1000);

            Thread = new Thread(() =>
            {
                try
                {
                    Thread.Sleep(10000);

                    var time = Call<int>("GetGameTime");

                    while (!IsFinished())
                    {
                        var ngt = Call<int>("GetGameTime");

                        if (ngt > time)
                        {
                            LastProgressTimeUtc = DateTime.UtcNow;
                            time = ngt;
                        }

                        if (Stopping)
                        {
                            return;
                        }

                        Thread.Sleep(1000);
                    }

                    Call("QuitGame");
                    Thread.Sleep(1000);
                    Finished = true;
                    Client.Close();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    Finished = false;
                }
            });

            Thread.IsBackground = true;
            Thread.Start();
        }

        private void Setup(bool minimized)
        {
            var api = Call<float>("GetApiVersion");
            Debug.WriteLine("Api: " + api.ToString());

            if (Call<bool>("GetGameInProgress"))
            {
                throw new Exception("Game already in progress.");
            }

            Call("ResetGameSettings");

            if (minimized)
            {
                Call("SetRunUnfocused", true);
                Call("SetUseInGameResolution", false);
                Call("SetWindowMinimized", true);
            }

            for (int i = 1; i <= 8; i++)
            {
                Call("SetPlayerClosed", i);
            }

            Call("SetGameType", (int)GameType);
            if (GameType == GameType.SCENARIO)
            {
                Call("SetGameScenarioName", ScenarioName);
            }

            Call("SetGameMapType", (int)MapType);
            Call("SetGameMapSize", (int)MapSize);
            Call("SetGameDifficulty", (int)Difficulty);
            Call("SetGameStartingResources", (int)StartingResources);
            Call("SetGamePopulationLimit", PopulationLimit);
            Call("SetGameRevealMap", (int)RevealMap);
            Call("SetGameStartingAge", (int)StartingAge);
            Call("SetGameVictoryType", (int)VictoryType, VictoryValue);
            Call("SetGameTeamsTogether", TeamsTogether);
            Call("SetGameTeamsLocked", LockTeams);
            Call("SetGameAllTechs", AllTechs);
            Call("SetGameRecorded", Recorded);

            foreach (var player in Players)
            {
                if (player.IsHuman)
                {
                    Call("SetPlayerHuman", player.PlayerNumber);
                }
                else
                {
                    Call("SetPlayerComputer", player.PlayerNumber, player.AiFile);
                }

                Call("SetPlayerCivilization", player.PlayerNumber, player.Civilization);
                Call("SetPlayerColor", player.PlayerNumber, player.Color);
                Call("SetPlayerTeam", player.PlayerNumber, player.Team);

                player.Exists = true;
                player.Alive = true;
                player.Score = 0;
            }
        }

        private bool IsFinished()
        {
            foreach (var player in Players)
            {
                player.Exists = Call<bool>("GetPlayerExists", player.PlayerNumber);
                player.Alive = Call<bool>("GetPlayerAlive", player.PlayerNumber);
                player.Score = Call<int>("GetPlayerScore", player.PlayerNumber);
            }

            var time = Call<int>("GetGameTime");
            var progress = Call<bool>("GetGameInProgress");

            if (time > 10 && !progress)
            {
                return true;
            }

            var team = new int[5];
            foreach (var player in Players.Where(p => p.Alive && (int)p.Team <= 4))
            {
                team[(int)player.Team]++;
            }

            if (team.Count(t => t > 0) > 1)
            {
                return false;
            }
            else if (team[0] > 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private IEnumerable<Player> GetWinners()
        {
            if (!Finished)
            {
                throw new Exception("Game isn't finished yet.");
            }

            var liveteams = new HashSet<int>() { 0 };

            foreach (var player in Players)
            {
                if (player.Alive)
                {
                    liveteams.Add((int)player.Team);
                }
            }

            return Players.Where(p => liveteams.Contains((int)p.Team));
        }

        private void Call(string method, params object[] arguments)
        {
            Call<object>(method, arguments);
        }

        private T Call<T>(string method, params object[] arguments)
        {
            if (Client == null)
            {
                throw new Exception("Client == null");
            }

            lock (Client)
            {
                if (!Client.Connected)
                {
                    throw new Exception("Client not connected");
                }

                var message = new object[] { 0, NextId++, method, arguments };
                var bin = MessagePackSerializer.Serialize(message);

                var stream = Client.GetStream();

                stream.Write(bin);

                using (var reader = new MessagePackStreamReader(stream, true))
                {
                    if (reader.ReadAsync(CancellationToken.None).Result is ReadOnlySequence<byte> msgpack)
                    {
                        //Debug.WriteLine(MessagePackSerializer.ConvertToJson(msgpack));
                        var response = (object[])MessagePackSerializer.Deserialize<object>(msgpack);

                        return (T)Convert.ChangeType(response[3], typeof(T));
                    }
                    else
                    {
                        throw new Exception("Call failed");
                    }
                }
            }
        }
    }
}
