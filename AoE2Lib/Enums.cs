﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib
{
    public enum TypeOp
    {
        C = 6, G = 2
    }

    public enum MathOp
    {
        S_EQUALS = 0, S_ADD = 1, G_MUL = 15, G_DIV = 16, G_MIN = 17, G_MAX = 18, G_MOD = 19, C_EQUALS = 24, C_MIN = 29, C_MAX = 30
    }

    public enum FactId
    {
        GAME_TIME, POPULATION_CAP, POPULATION_HEADROOM, HOUSING_HEADROOM, IDLE_FARM_COUNT, FOOD_AMOUNT, WOOD_AMOUNT, STONE_AMOUNT,
        GOLD_AMOUNT, ESCROW_AMOUNT, COMMODITY_BUYING_PRICE, COMMODITY_SELLING_PRICE, DROPSITE_MIN_DISTANCE,
        SOLDIER_COUNT, ATTACK_SOLDIER_COUNT, DEFEND_SOLDIER_COUNT, WARBOAT_COUNT, ATTACK_WARBOAT_COUNT,
        DEFEND_WARBOAT_COUNT, CURRENT_AGE, CURRENT_SCORE, CIVILIZATION, PLAYER_NUMBER, PLAYER_IN_GAME, UNIT_COUNT,
        UNIT_TYPE_COUNT, UNIT_TYPE_COUNT_TOTAL, BUILDING_COUNT, BUILDING_TYPE_COUNT, BUILDING_TYPE_COUNT_TOTAL,
        POPULATION, MILITARY_POPULATION, CIVILIAN_POPULATION, RANDOM_NUMBER, RESOURCE_AMOUNT, PLAYER_DISTANCE,
        ALLIED_GOAL, ALLIED_SN, RESOURCE_PERCENT, ENEMY_BUILDINGS_IN_TOWN, ENEMY_UNITS_IN_TOWN, ENEMY_VILLAGERS_IN_TOWN,
        PLAYERS_IN_GAME, DEFENDER_COUNT, BUILDING_TYPE_IN_TOWN, UNIT_TYPE_IN_TOWN, VILLAGER_TYPE_IN_TOWN, GAIA_TYPE_COUNT,
        GAIA_TYPE_COUNT_TOTAL, CC_GAIA_TYPE_COUNT, CURRENT_AGE_TIMER, TIMER_STATUS, PLAYERS_TRIBUTE, PLAYERS_TRIBUTE_MEMORY,
        TREATY_TIME
    }

    public enum ObjectData
    {
        INDEX = -1, ID, TYPE, CLASS, CATEGORY, CMDID, ACTION, ORDER, TARGET, POINT_X, POINT_Y, HITPOINTS,
        MAXHP, RANGE, SPEED, DROPSITE, RESOURCE, CARRY, GARRISONED, GARRISON_COUNT, STATUS, PLAYER, ATTACK_STANCE,
        ACTION_TIME, TARGET_ID, FORMATION_ID, PATROLLING, STRIKE_ARMOR, PIERCE_ARMOR, BASE_ATTACK, LOCKED,
        GARRISON_ID, TRAIN_COUNT, TASKS_COUNT, ATTACKER_COUNT, ATTACKER_ID, UNDER_ATTACK, ATTACK_TIMER, POINT_Z, PRECISE_X,
        PRECISE_Y, PRECISE_Z, RESEARCHING, TILE_POSITION, TILE_INVERSE, DISTANCE, PRECISE_DISTANCE, FULL_DISTANCE, MAP_ZONE_ID,
        ON_MAINLAND, IDLING, MOVE_X, MOVE_Y, PRECISE_MOVE_X, PRECISE_MOVE_Y, RELOAD_TIME, NEXT_ATTACK, TRAIN_SITE,
        TRAIN_TIME, BLAST_RADIUS, BLAST_LEVEL, PROGRESS_TYPE, PROGRESS_VALUE, MIN_RANGE, TARGET_TIME, HERESY,
        FAITH, REDEMPTION, ATONEMENT, THEOCRACY, SPIES, BALLISTICS, GATHER_TYPE, LANGUAGE_ID, GROUP_FLAG,
        HERO_FLAGS, HERO, AUTO_HEAL, NO_CONVERT, FRAME_DELAY, ATTACK_COUNT, TO_PRECISE, BASE_TYPE, UPGRADE_TYPE,
        OWNERSHIP, CAPTURE_FLAG
    }

    public enum CmdId
    {
        FLAG, LIVESTOCK_GAIA, CIVILIAN_BUILDING, VILLAGER, MILITARY, TRADE, MONK, TRANSPORT, RELIC, FISHING_SHIP, MILITARY_BUILDING
    }

    public enum StrategicNumber
    {
        PERCENT_CIVILIAN_EXPLORERS = 0,
        PERCENT_CIVILIAN_BUILDERS = 1,
        PERCENT_CIVILIAN_GATHERERS = 2,
        CAP_CIVILIAN_EXPLORERS = 3,
        CAP_CIVILIAN_BUILDERS = 4,
        CAP_CIVILIAN_GATHERERS = 5,
        MINIMUM_ATTACK_GROUP_SIZE = 16,
        TOTAL_NUMBER_EXPLORERS = 18,
        PERCENT_ENEMY_SIGHTED_RESPONSE = 19,
        ENEMY_SIGHTED_RESPONSE_DISTANCE = 20,
        SENTRY_DISTANCE = 22,
        RELIC_RETURN_DISTANCE = 23,
        MINIMUM_DEFEND_GROUP_SIZE = 25,
        MAXIMUM_ATTACK_GROUP_SIZE = 26,
        MAXIMUM_DEFEND_GROUP_SIZE = 28,
        MINIMUM_PEACE_LIKE_LEVEL = 29,
        PERCENT_EXPLORATION_REQUIRED = 32,
        ZERO_PRIORITY_DISTANCE = 34,
        MINIMUM_CIVILIAN_EXPLORERS = 35,
        NUMBER_ATTACK_GROUPS = 36,
        NUMBER_DEFEND_GROUPS = 38,
        ATTACK_GROUP_GATHER_SPACING = 41,
        NUMBER_EXPLORE_GROUPS = 42,
        MINIMUM_EXPLORE_GROUP_SIZE = 43,
        MAXIMUM_EXPLORE_GROUP_SIZE = 44,
        GOLD_DEFEND_PRIORITY = 50,
        STONE_DEFEND_PRIORITY = 51,
        FORAGE_DEFEND_PRIORITY = 52,
        TOWN_DEFEND_PRIORITY = 56,
        DEFENSE_DISTANCE = 57,
        SENTRY_DISTANCE_VARIATION = 72,
        MINIMUM_TOWN_SIZE = 73,
        MAXIMUM_TOWN_SIZE = 74,
        GROUP_COMMANDER_SELECTION_METHOD = 75,
        CONSECUTIVE_IDLE_UNIT_LIMIT = 76,
        TARGET_EVALUATION_DISTANCE = 77,
        TARGET_EVALUATION_HITPOINTS = 78,
        TARGET_EVALUATION_DAMAGE_CAPABILITY = 79,
        TARGET_EVALUATION_KILLS = 80,
        TARGET_EVALUATION_ALLY_PROXIMITY = 81,
        TARGET_EVALUATION_ROF = 82,
        TARGET_EVALUATION_RANDOMNESS = 83,
        CAMP_MAX_DISTANCE = 86,
        MILL_MAX_DISTANCE = 87,
        TARGET_EVALUATION_ATTACK_ATTEMPTS = 89,
        TARGET_EVALUATION_RANGE = 90,
        DEFEND_OVERLAP_DISTANCE = 92,
        SCALE_MINIMUM_ATTACK_GROUP_SIZE = 93,
        SCALE_MAXIMUM_ATTACK_GROUP_SIZE = 94,
        ATTACK_GROUP_SIZE_RANDOMNESS = 98,
        SCALING_FREQUENCY = 99,
        MAXIMUM_GAIA_ATTACK_RESPONSE = 100,
        BUILD_FREQUENCY = 101,
        ATTACK_INTELLIGENCE = 103,
        INITIAL_ATTACK_DELAY = 104,
        SAVE_SCENARIO_INFORMATION = 105,
        SPECIAL_ATTACK_TYPE1 = 106,
        SPECIAL_ATTACK_TYPE2 = 107,
        SPECIAL_ATTACK_TYPE3 = 108,
        SPECIAL_ATTACK_INFLUENCE1 = 109,
        NUMBER_BUILD_ATTEMPTS_BEFORE_SKIP = 114,
        MAX_SKIPS_PER_ATTEMPT = 115,
        FOOD_GATHERER_PERCENTAGE = 117,
        GOLD_GATHERER_PERCENTAGE = 118,
        STONE_GATHERER_PERCENTAGE = 119,
        WOOD_GATHERER_PERCENTAGE = 120,
        TARGET_EVALUATION_CONTINENT = 122,
        TARGET_EVALUATION_SIEGE_WEAPON = 123,
        GROUP_LEADER_DEFENSE_DISTANCE = 131,
        INITIAL_ATTACK_DELAY_TYPE = 134,
        INTELLIGENT_GATHERING = 142,
        TASK_UNGROUPED_SOLDIERS = 143,
        TARGET_EVALUATION_BOAT = 144,
        NUMBER_ENEMY_OBJECTS_REQUIRED = 145,
        NUMBER_MAX_SKIP_CYCLES = 146,
        RETASK_GATHER_AMOUNT = 148,
        MAX_RETASK_GATHER_AMOUNT = 149,
        MAX_BUILD_PLAN_GATHERER_PERCENTAGE = 160,
        FOOD_DROPSITE_DISTANCE = 163,
        WOOD_DROPSITE_DISTANCE = 164,
        STONE_DROPSITE_DISTANCE = 165,
        GOLD_DROPSITE_DISTANCE = 166,
        INITIAL_EXPLORATION_REQUIRED = 167,
        RANDOM_PLACEMENT_FACTOR = 168,
        REQUIRED_FOREST_TILES = 169,
        PERCENT_HALF_EXPLORATION = 179,
        TARGET_EVALUATION_TIME_KILL_RATIO = 184,
        TARGET_EVALUATION_IN_PROGRESS = 185,
        COOP_SHARE_INFORMATION = 194,
        PERCENTAGE_EXPLORE_EXTERMINATORS = 198,
        TRACK_PLAYER_HISTORY = 201,
        MINIMUM_DROPSITE_BUFFER = 202,
        USE_BY_TYPE_MAX_GATHERING = 203,
        MINIMUM_BOAR_HUNT_GROUP_SIZE = 204,
        MINIMUM_AMOUNT_FOR_TRADING = 216,
        EASIEST_REACTION_PERCENTAGE = 218,
        EASIER_REACTION_PERCENTAGE = 219,
        ALLOW_CIVILIAN_DEFENSE = 225,
        NUMBER_FORWARD_BUILDERS = 226,
        PERCENT_ATTACK_SOLDIERS = 227,
        PERCENT_ATTACK_BOATS = 228,
        DO_NOT_SCALE_FOR_DIFFICULTY_LEVEL = 229,
        GROUP_FORM_DISTANCE = 230,
        IGNORE_ATTACK_GROUP_UNDER_ATTACK = 231,
        GATHER_DEFENSE_UNITS = 232,
        MAXIMUM_WOOD_DROP_DISTANCE = 233,
        MAXIMUM_FOOD_DROP_DISTANCE = 234,
        MAXIMUM_HUNT_DROP_DISTANCE = 235,
        MAXIMUM_GOLD_DROP_DISTANCE = 237,
        MAXIMUM_STONE_DROP_DISTANCE = 238,
        GATHER_IDLE_SOLDIERS_AT_CENTER = 239,
        ENABLE_NEW_BUILDING_SYSTEM = 242,
        PERCENT_BUILDING_CANCELLATION = 243,
        ENABLE_BOAR_HUNTING = 244,
        MINIMUM_NUMBER_HUNTERS = 245,
        OBJECT_REPAIR_LEVEL = 246,
        ENABLE_PATROL_ATTACK = 247,
        DROPSITE_SEPARATION_DISTANCE = 248,
        TARGET_PLAYER_NUMBER = 249,
        SAFE_TOWN_SIZE = 250,
        FOCUS_PLAYER_NUMBER = 251,
        MINIMUM_BOAR_LURE_GROUP_SIZE = 252,
        PREFERRED_MILL_PLACEMENT = 253,
        ENABLE_OFFENSIVE_PRIORITY = 254,
        BUILDING_TARGETING_MODE = 255,
        HOME_EXPLORATION_TIME = 256,
        NUMBER_CIVILIAN_MILITIA = 257,
        ALLOW_CIVILIAN_OFFENSE = 258,
        PREFERRED_TRADE_DISTANCE = 259,
        LUMBER_CAMP_MAX_DISTANCE = 260,
        MINING_CAMP_MAX_DISTANCE = 261,
        WALL_TARGETING_MODE = 262,
        LIVESTOCK_TO_TOWN_CENTER = 263,
        ENABLE_TRAINING_QUEUE = 264,
        IGNORE_TOWER_ELEVATION = 265,
        TOWN_CENTER_PLACEMENT = 266,
        DISABLE_TOWER_PRIORITY = 267,
        PLACEMENT_ZONE_SIZE = 268,
        PLACEMENT_FAIL_DELTA = 269,
        PLACEMENT_TO_CENTER = 270,
        DISABLE_ATTACK_GROUPS = 271,
        ALLOW_ADJACENT_DROPSITES = 272,
        DEFER_DROPSITE_UPDATE = 273,
        MAXIMUM_GARRISON_FILL = 274,
        NUMBER_GARRISON_UNITS = 275,
        FILTER_UNDER_ATTACK = 276,
        DISABLE_DEFEND_GROUPS = 277,
        DOCK_PLACEMENT_MODE = 278,
        DOCK_PROXIMITY_FACTOR = 279,
        DOCK_AVOIDANCE_FACTOR = 280,
        DOCK_TRAINING_FILTER = 281,
        FREE_SIEGE_TARGETING = 282,
        WARSHIP_TARGETING_MODE = 283,
        DISABLE_SIGHTED_RESPONSE_CAP = 284,
        DISABLE_BUILDER_ASSISTANCE = 285,
        LOCAL_TARGETING_MODE = 286,
        LIVESTOCK_DEFEND_PRIORITY = 287,
        NUMBER_TASKED_UNITS = 288,
        DISABLE_VILLAGER_GARRISON = 291,
        TARGET_POINT_ADJUSTMENT = 292,
        UNEXPLORED_CONSTRUCTION = 293,
        DISABLE_TRADE_EVASION = 294,
        BOAR_LURE_DESTINATION = 295
    }

    public enum UnitClass
    {
        Archer = 900,
        Artifact,
        TradeBoat,
        Building,
        Civilian,
        OceanFish,
        Infantry,
        BerryBush,
        StoneMine,
        PreyAnimal,
        PredatorAnimal,
        Miscellaneous,
        Cavalry,
        SiegeWeapon,
        Terrain,
        Tree,
        TreeStump,
        Healer,
        Monk,
        TradeCart,
        TransportBoat,
        FishingBoat,
        Warship,
        Conquistador,
        WarElephant,
        Hero,
        ElephantArcher,
        Wall,
        Phalanx,
        DomesticAnimal,
        Flag,
        DeepSeaFish,
        GoldMine,
        ShoreFish,
        Cliff,
        Petard,
        CavalryArcher,
        Doppelganger,
        Bird,
        Gate,
        SalvagePile,
        ResourcePile,
        Relic,
        MonkWithRelic,
        HandCannoneer,
        TwoHandedSwordsMan,
        Pikeman,
        Scout,
        OreMine,
        Farm,
        Spearman,
        PackedUnit,
        Tower,
        BoardingBoat,
        UnpackedSiegeUnit,
        Ballista,
        Raider,
        CavalryRaider,
        Livestock,
        King,
        MiscBuilding,
        ControlledAnimal
    }

    public enum UnitAction
    {
        DEFAULT, MOVE, PATROL, GUARD, FOLLOW, STOP, GROUND, GARRISON, DELETE, UNLOAD, TRAIN, GATHER, LOCK, WORK, UNGARRISON, DROP_RELIC, PACK, UNPACK, NONE
    }

    public enum UnitOrder
    {
        NONE = -1, ATTACK, DEFEND, BUILD, HEAL, CONVERT, EXPLORE, STOP,
        RUNAWAY, RETREAT, GATHER, MOVE, PATROL, FOLLOW, HUNT, TRANSPORT,
        TRADE, EVADE, ENTER, REPAIR, TRAIN, RESEARCH, UNLOAD, RELIC = 31
    }

    public enum UnitFormation
    {
        LINE = 2, BOX = 4, STAGGER = 7, FLANK = 8
    }

    public enum UnitStance
    {
        AGGRESSIVE, DEFENSIVE, STAND_GROUND, NO_ATTACK
    }

    public enum PlayerStance
    {
        ALLY, NEUTRAL, ENEMY = 3
    }

    public enum PositionType
    {
        CENTER, OPPOSITE, CORNER, ENEMY, BORDER, MIRROR, FLANK, ZERO, MAP_SIZE, SELF, TARGET, FOCUS, OBJECT, POINT
    }

    public enum UnitFindType
    {
        MILLITARY, CIVILIAN, BUILDING, WOOD, FOOD, GOLD, STONE, ALL
    }

}
