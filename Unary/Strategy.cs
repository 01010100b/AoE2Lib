using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.GameElements;
using Unary.Utils;

namespace Unary
{
    public abstract class Strategy
    {
        public abstract void Update(Bot bot);
        internal void UpdateInternal(Bot bot)
        {
            SetStrategicNumbers(bot.GameState.StrategicNumbers);

            Update(bot);
        }

        private void SetStrategicNumbers(Dictionary<StrategicNumber, int> sns)
        {
            sns[StrategicNumber.PERCENT_CIVILIAN_EXPLORERS] = 0;
            sns[StrategicNumber.CAP_CIVILIAN_EXPLORERS] = 0;
            sns[StrategicNumber.TOTAL_NUMBER_EXPLORERS] = 1;
            sns[StrategicNumber.NUMBER_EXPLORE_GROUPS] = 1;

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

            sns[StrategicNumber.TOWN_CENTER_PLACEMENT] = 584;
            sns[StrategicNumber.ENABLE_NEW_BUILDING_SYSTEM] = 1;
            sns[StrategicNumber.PERCENT_BUILDING_CANCELLATION] = 0;
            sns[StrategicNumber.DISABLE_BUILDER_ASSISTANCE] = 1;
            sns[StrategicNumber.DEFER_DROPSITE_UPDATE] = 1;
            sns[StrategicNumber.DROPSITE_SEPARATION_DISTANCE] = 4;
            sns[StrategicNumber.MILL_MAX_DISTANCE] = 20;
            sns[StrategicNumber.CAMP_MAX_DISTANCE] = 20;
            sns[StrategicNumber.CAP_CIVILIAN_BUILDERS] = 4;

            sns[StrategicNumber.INTELLIGENT_GATHERING] = 1;
            sns[StrategicNumber.USE_BY_TYPE_MAX_GATHERING] = 1;
            sns[StrategicNumber.MAXIMUM_WOOD_DROP_DISTANCE] = 7;
            sns[StrategicNumber.MAXIMUM_GOLD_DROP_DISTANCE] = 4;
            sns[StrategicNumber.MAXIMUM_STONE_DROP_DISTANCE] = 4;
            sns[StrategicNumber.MAXIMUM_FOOD_DROP_DISTANCE] = 3;
            sns[StrategicNumber.MAXIMUM_HUNT_DROP_DISTANCE] = 3;
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
