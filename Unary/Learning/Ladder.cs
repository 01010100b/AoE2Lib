using AoE2Lib.Bots;
using AoE2Lib.Games;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Unary.Learning
{
    internal class Ladder
    {
        private class Participant
        {
            public string Name { get; set; }
            public int Elo { get; set; }
            public int Games { get; set; }
        }

        public static int GetEloDelta(int elo_winner, int elo_loser, int k = 32)
        {
            var exp_winner = 1d / (1d + Math.Pow(10, (elo_loser - elo_winner) / 400d));

            return Math.Max(1, (int)Math.Round(k * (1 - exp_winner)));
        }

        private const int STARTING_ELO = 1000;
        private const int MIN_ELO = 100;
        private const string SAVE_FILE = @"G:\aoe2\ladder.json";

        private readonly List<Participant> Participants = new();
        private readonly Random Rng = new();
        private volatile bool Stopping = false;

        public Ladder()
        {
            Load();
        }

        public void AddPlayer(string name)
        {
            if (Participants.Select(p => p.Name).Contains(name))
            {
                return;
            }

            var participant = new Participant()
            {
                Name = name,
                Elo = STARTING_ELO,
                Games = 0
            };

            Participants.Add(participant);
            Save();
        }

        public void Run(string exe, params Type[] bot_types)
        {
            foreach (var bot in bot_types)
            {
                var btype = bot.BaseType;
                var good = false;

                while (btype != null)
                {
                    if (btype == typeof(Bot))
                    {
                        good = true;

                        break;
                    }

                    btype = btype.BaseType;
                }

                if (!good)
                {
                    throw new ArgumentException($"Bot type {bot.Name} does not extend the Bot class");
                }

                var ctr = bot.GetConstructor(Type.EmptyTypes);

                if (ctr == null)
                {
                    throw new ArgumentException($"Bot type {bot.Name} does not have a parameterless constructor");
                }
            }

            if (Participants.Count < 2)
            {
                throw new Exception("Need at least 2 participants");
            }

            Stopping = false;

            var bots = new Dictionary<string, Type>();

            foreach (var type in bot_types)
            {
                var bot = (Bot)Activator.CreateInstance(type);
                var name = bot.Name;

                bots.Add(name, type);
            }

            var games = new List<Game>();
            var queue = new ConcurrentQueue<KeyValuePair<Game, Dictionary<int, Bot>>>();
            var runners = new List<InstanceRunner>();
            var count = Math.Max(1, Environment.ProcessorCount - 1);

            for (int i = 0; i < count; i++)
            {
                var runner = new InstanceRunner(exe);

                runners.Add(runner);
                runner.Start(queue);
            }

            while (!Stopping)
            {
                if (queue.Count == 0)
                {
                    var game = GetNextGame(bots);
                    var dict = new Dictionary<int, Bot>();

                    foreach (var player in game.GetPlayers())
                    {
                        var name = player.AiFile;

                        if (bots.TryGetValue(name, out var type))
                        {
                            var bot = (Bot)Activator.CreateInstance(type);

                            dict.Add(player.PlayerNumber, bot);
                        }
                    }

                    games.Add(game);
                    queue.Enqueue(new KeyValuePair<Game, Dictionary<int, Bot>>(game, dict));
                }
                else
                {
                    var finished = games.Where(g => g.Finished).ToList();

                    foreach (var game in finished)
                    {
                        AddResult(game);
                    }

                    games.RemoveAll(g => finished.Contains(g));
                }

                Thread.Sleep(1000);
            }

            while (queue.TryDequeue(out var g))
            {
                games.Remove(g.Key);
            }

            foreach (var game in games)
            {
                while (!game.Finished)
                {
                    Thread.Sleep(1000);
                }

                AddResult(game);
            }

            foreach (var runner in runners)
            {
                runner.Stop();
            }

            Save();
            Stopping = false;
        }

        public void Stop()
        {
            Stopping = true;
        }

        private Game GetNextGame(Dictionary<string, Type> bots)
        {
            var teams = new[] { 1, 2, 4 };
            var team = teams[Rng.Next(teams.Length)];
            var player1 = Participants[Rng.Next(Participants.Count)];
            var player2 = Participants[Rng.Next(Participants.Count)];

            for (int i = 0; i < 4; i++)
            {
                var p = Participants[Rng.Next(Participants.Count)];

                if (p.Games < 0.7 * player1.Games)
                {
                    player1 = p;
                }

                p = Participants[Rng.Next(Participants.Count)];

                if (p.Games < 0.7 * player2.Games)
                {
                    player2 = p;
                }
            }

            for (int i = 0; i < 4; i++)
            {
                var p = Participants[Rng.Next(Participants.Count)];

                if (bots.ContainsKey(p.Name) && !bots.ContainsKey(player1.Name))
                {
                    player1 = p;
                }

                p = Participants[Rng.Next(Participants.Count)];

                if (bots.ContainsKey(p.Name) && !bots.ContainsKey(player2.Name))
                {
                    player2 = p;
                }
            }

            while (player2 == player1)
            {
                player2 = Participants[Rng.Next(Participants.Count)];
            }

            var mapsize = MapSize.TINY;

            switch (team)
            {
                case 2: mapsize = MapSize.MEDIUM; break;
                case 3: mapsize = MapSize.NORMAL; break;
                case 4: mapsize = MapSize.LARGE; break;
            }

            var game = new Game()
            {
                GameType = GameType.RANDOM_MAP,
                MapType = MapType.ARABIA,
                MapSize = mapsize,
                Difficulty = Difficulty.HARD,
                StartingResources = StartingResources.STANDARD,
                PopulationLimit = 200,
                RevealMap = RevealMap.NORMAL,
                StartingAge = StartingAge.STANDARD,
                VictoryType = VictoryType.TIME_LIMIT,
                VictoryValue = 2 * 60 * 60 * 2,
                TeamsTogether = true,
                LockTeams = true,
                AllTechs = false,
                Recorded = false
            };

            var slot = 1;

            for (int i = 0; i < team; i++)
            {
                var player = new Player()
                {
                    PlayerNumber = slot,
                    IsHuman = false,
                    AiFile = player1.Name,
                    Civilization = (int)Civilization.RANDOM,
                    Team = Team.TEAM_1,
                    Color = (Color)slot,
                };

                game.AddPlayer(player);
                slot++;
            }

            for (int i = 0; i < team; i++)
            {
                var player = new Player()
                {
                    PlayerNumber = slot,
                    IsHuman = false,
                    AiFile = player2.Name,
                    Civilization = (int)Civilization.RANDOM,
                    Team = Team.TEAM_2,
                    Color = (Color)slot,
                };

                game.AddPlayer(player);
                slot++;
            }

            return game;
        }

        private void AddResult(Game game)
        {
            var winners = game.Winners.ToList();
            var losers = game.GetPlayers().Where(p => !winners.Contains(p)).ToList();

            if (winners.Count == 0 || losers.Count == 0)
            {
                return;
            }

            var winner = Participants.Single(p => winners[0].AiFile == p.Name);
            var loser = Participants.Single(p => losers[0].AiFile == p.Name);
            var delta_elo = Math.Min(loser.Elo - MIN_ELO, GetEloDelta(winner.Elo, loser.Elo));

            winner.Elo += delta_elo;
            loser.Elo -= delta_elo;
            winner.Games++;
            loser.Games++;

            Save();
        }

        private void Load()
        {
            if (File.Exists(SAVE_FILE))
            {
                var participants = Program.Deserialize<List<Participant>>(SAVE_FILE);

                Participants.Clear();
                Participants.AddRange(participants);
            }
        }

        private void Save()
        {
            Participants.Sort((a, b) => b.Elo.CompareTo(a.Elo));
            Program.Serialize(Participants, SAVE_FILE);
        }
    }
}
