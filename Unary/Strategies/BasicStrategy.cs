using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Strategies
{
    public class BasicStrategy : Strategy
    {
        private readonly Random RNG = new Random(Guid.NewGuid().GetHashCode());

        public override void Update(Bot bot)
        {
            var sw = new Stopwatch();
            sw.Start();

            SetStrategicNumbers(bot);
            Economy(bot);

            sw.Stop();
            Log.Debug($"tick ms: {sw.ElapsedMilliseconds}");
        }

        private void Economy(Bot bot)
        {
            var me = bot.GameState.Players[bot.GameState.PlayerNumber];

            if (me.Population >= 0)
            {
                bot.BuildModule.MaxBuildRange = 20 + (me.Population / 10);
                bot.BuildModule.MaxMillRange = bot.BuildModule.MaxBuildRange + 10;
                bot.BuildModule.MaxLumberRange = bot.BuildModule.MaxBuildRange + 15;

                var trees = bot.GameState.Units.Values.Where(u => u.Class == UnitClass.Tree && u.Targetable).ToList();
                if (trees.Count > 10)
                {
                    trees.Sort((a, b) => a.Position.DistanceTo(bot.GameState.MyPosition).CompareTo(b.Position.DistanceTo(bot.GameState.MyPosition)));
                    var count = 50 + me.Population;
                    var tree = trees[RNG.Next(Math.Min(count, trees.Count))];
                    bot.BuildModule.MaxLumberRange = (int)Math.Max(bot.BuildModule.MaxLumberRange, tree.Position.DistanceTo(bot.GameState.MyPosition));
                }
            }

            bot.TrainModule.Train(bot.Mod.Villager);
            bot.ResearchModule.Research(bot.Mod.TechDefs[22]);

            if (me.HousingHeadroom < 5 && me.PopulationHeadroom > 0)
            {
                bot.BuildModule.Build(bot.Mod.House, bot, 100, 2);
            }

            var lumbercamps = 1;
            if (me.CivilianPopulation > 10)
            {
                lumbercamps = me.CivilianPopulation / 10;
            }
            if (bot.GameState.GameTime < TimeSpan.FromMinutes(2) || bot.GameState.Units.Values.Count(u => u.Class == UnitClass.Tree) < 10)
            {
                lumbercamps = 0;
            }

            bot.BuildModule.Build(bot.Mod.LumberCamp, bot, lumbercamps, 1);

            var mills = 1;
            if (me.CivilianPopulation > 10)
            {
                mills = me.CivilianPopulation / 7;
            }
            if (bot.GameState.GameTime < TimeSpan.FromMinutes(3))
            {
                mills = 0;
            }

            bot.BuildModule.Build(bot.Mod.Mill, bot, mills, 1);

            var farms = 0;
            if (bot.GameState.GameTime > TimeSpan.FromMinutes(8))
            {
                farms = Math.Max(1, me.CivilianPopulation / 2);
            }
            if (bot.GameState.GetObjectTypeCountTotal(bot.Mod.Mill.BaseId) == 0)
            {
                farms = 0;
            }

            bot.BuildModule.Build(bot.Mod.Farm, bot, farms, 3);

            var camps = bot.GameState.Units.Values.Where(u => u.PlayerNumber == bot.GameState.PlayerNumber && u.BaseTypeId == bot.Mod.LumberCamp.BaseId).ToList();

            if (camps.Count > 0)
            {
                var camp = camps[RNG.Next(camps.Count)];
                var trees = bot.GameState.GetUnitsInRange(camp.Position, 4).Count(u => u.Class == UnitClass.Tree && u.Targetable);

                if (trees == 0)
                {
                    bot.MicroModule.TargetObject(camp.Id, camp.Id, UnitAction.DELETE, UnitStance.AGGRESSIVE);
                }
            }
        }

        private void SetStrategicNumbers(Bot bot)
        {
            var sns = bot.GameState.StrategicNumbers;
            var me = bot.GameState.Players[bot.GameState.PlayerNumber];

            sns[StrategicNumber.PERCENT_CIVILIAN_EXPLORERS] = 0;
            sns[StrategicNumber.CAP_CIVILIAN_EXPLORERS] = 0;
            sns[StrategicNumber.TOTAL_NUMBER_EXPLORERS] = 1;
            sns[StrategicNumber.NUMBER_EXPLORE_GROUPS] = 1;

            if (me.CivilianPopulation > 0 && me.MilitaryPopulation == 0)
            {
                sns[StrategicNumber.PERCENT_CIVILIAN_EXPLORERS] = 50;
                sns[StrategicNumber.CAP_CIVILIAN_EXPLORERS] = 1;
            }

            sns[StrategicNumber.PERCENT_ENEMY_SIGHTED_RESPONSE] = 100;
            sns[StrategicNumber.ENEMY_SIGHTED_RESPONSE_DISTANCE] = 100;
            sns[StrategicNumber.ZERO_PRIORITY_DISTANCE] = 100;
            sns[StrategicNumber.ENABLE_OFFENSIVE_PRIORITY] = 1;
            sns[StrategicNumber.ENABLE_PATROL_ATTACK] = 1;
            sns[StrategicNumber.MINIMUM_ATTACK_GROUP_SIZE] = 1;
            sns[StrategicNumber.MAXIMUM_ATTACK_GROUP_SIZE] = 1;
            sns[StrategicNumber.DISABLE_DEFEND_GROUPS] = 8;
            sns[StrategicNumber.CONSECUTIVE_IDLE_UNIT_LIMIT] = 0;
            sns[StrategicNumber.WALL_TARGETING_MODE] = 1;

            sns[StrategicNumber.ENABLE_NEW_BUILDING_SYSTEM] = 1;
            sns[StrategicNumber.PERCENT_BUILDING_CANCELLATION] = 0;
            sns[StrategicNumber.DISABLE_BUILDER_ASSISTANCE] = 1;
            sns[StrategicNumber.CAP_CIVILIAN_BUILDERS] = 4;
            if (me.Population >= 0)
            {
                sns[StrategicNumber.CAP_CIVILIAN_BUILDERS] = 4 + (me.Population / 20);
            }

            sns[StrategicNumber.INTELLIGENT_GATHERING] = 1;
            sns[StrategicNumber.USE_BY_TYPE_MAX_GATHERING] = 1;
            sns[StrategicNumber.MAXIMUM_WOOD_DROP_DISTANCE] = 4;
            sns[StrategicNumber.MAXIMUM_GOLD_DROP_DISTANCE] = 4;
            sns[StrategicNumber.MAXIMUM_STONE_DROP_DISTANCE] = 4;
            sns[StrategicNumber.MAXIMUM_FOOD_DROP_DISTANCE] = 4;
            sns[StrategicNumber.MAXIMUM_HUNT_DROP_DISTANCE] = 10;
            sns[StrategicNumber.ENABLE_BOAR_HUNTING] = 0;
            sns[StrategicNumber.LIVESTOCK_TO_TOWN_CENTER] = 1;

            sns[StrategicNumber.HOME_EXPLORATION_TIME] = 600;

            sns[StrategicNumber.FOOD_GATHERER_PERCENTAGE] = 80;
            sns[StrategicNumber.WOOD_GATHERER_PERCENTAGE] = 20;
            sns[StrategicNumber.GOLD_GATHERER_PERCENTAGE] = 0;
            sns[StrategicNumber.STONE_GATHERER_PERCENTAGE] = 0;
        }
    }
}
