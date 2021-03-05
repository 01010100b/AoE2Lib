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

namespace AoE2Lib
{
    public class Game
    {
        public class Player
        {
            public int PlayerNumber { get; set; }
            public bool IsHuman { get; set; }
            public string AiFile { get; set; }
            public int Civilization { get; set; }
            public int Color { get; set; }
            public int Team { get; set; }
            public bool Exists { get; internal set; }
            public bool Alive { get; internal set; }
            public int Score { get; internal set; }
        }

        public int GameType { get; set; } = 0;
        public string ScenarioName { get; set; } = null;
        public int MapType { get; set; } = 9;
        public int MapSize { get; set; } = 0;
        public int Difficulty { get; set; } = 1;
        public int StartingResources { get; set; } = 0;
        public int PopulationLimit { get; set; } = 200;
        public int RevealMap { get; set; } = 0;
        public int StartingAge { get; set; } = 0;
        public int VictoryType { get; set; } = 0;
        public int VictoryValue { get; set; } = 0;
        public bool TeamsTogether { get; set; } = true;
        public bool LockTeams { get; set; } = false;
        public bool AllTechs { get; set; } = false;
        public bool Recorded { get; set; } = true;
        public List<Player> Players { get; set; } = new List<Player>();

        private readonly TcpClient Client = new TcpClient();
        private int NextId { get; set; } = 1;

        internal void Start(int port)
        {
            Client.Connect(new IPEndPoint(IPAddress.Loopback, port));

            var api = Call<float>("GetApiVersion");
            Debug.WriteLine("Api: " + api.ToString());
            

            if (Call<bool>("GetGameInProgress"))
            {
                throw new Exception("Game already in progress.");
            }

            Call("ResetGameSettings");

            for (int i = 1; i <= 8; i++)
            {
                Call("SetPlayerClosed", i);
            }

            Call("SetGameType", GameType);
            if (GameType == 3)
            {
                Call("SetGameScenarioName", ScenarioName);
            }

            Call("SetGameMapType", MapType);
            Call("SetGameMapSize", MapSize);
            Call("SetGameDifficulty", Difficulty);
            Call("SetGameStartingResources", StartingResources);
            Call("SetGamePopulationLimit", PopulationLimit);
            Call("SetGameRevealMap", RevealMap);
            Call("SetGameStartingAge", StartingAge);
            Call("SetGameVictoryType", VictoryType, VictoryValue);
            Call("SetGameTeamsTogether", TeamsTogether);
            Call("SetGameLockTeams", LockTeams);
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

            Call("StartGame");

            while(!IsFinished())
            {
                Thread.Sleep(1000);
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

            var team = new int[5];
            foreach (var player in Players.Where(p => p.Alive))
            {
                team[player.Team]++;
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

            if (!Client.Connected)
            {
                throw new Exception("Client not connected");
            }

            lock (Client)
            {
                var message = new object[] { 0, NextId++, method, arguments };
                var bin = MessagePackSerializer.Serialize(message);

                var stream = Client.GetStream();

                stream.Write(bin);

                using (var reader = new MessagePackStreamReader(stream, true))
                {
                    if (reader.ReadAsync(CancellationToken.None).Result is ReadOnlySequence<byte> msgpack)
                    {
                        Debug.WriteLine(MessagePackSerializer.ConvertToJson(msgpack));

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
