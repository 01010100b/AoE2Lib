﻿using System;
using System.Collections.Generic;
using System.Text;

namespace GameRunner
{
    public enum GameType
    {
        RANDOM_MAP, REGICIDE, DEATH_MATCH, SCENARIO, KING_OF_THE_HILL = 5, WONDER_RACE, TURBO_RANDOM_MAP = 8
    }

    public enum MapType
    {
        ARABIA = 9, ARCHIPELAGO, BALTIC, BLACK_FOREST, COASTAL, CONTINENTAL, CRATER_LAKE, FORTRESS, GOLD_RUSH, HIGHLAND, ISLANDS, MEDITERRANEAN, MIGRATION, RIVERS, TEAM_ISLANDS, RANDOM_MAP, SCANDINAVIA, MONGOLIA, YUCATAN, SALT_MARSH, ARENA, OASIS = 31, GHOST_LAKE, NOMAD, IBERIA, BRITAIN, MIDEAST, TEXAS, ITALY, CENTRAL_AMERICA, FRANCE, NORSE_LANDS, SEA_OF_JAPAN, BYZANTIUM, RANDOM_LAND_MAP = 45, RANDOM_REAL_WORLD_MAP = 47, BLIND_RANDOM, CONVENTIONAL_RANDOM_MAP
    }

    public enum MapSize
    {
        TINY, SMALL, MEDIUM, NORMAL, LARGE, GIANT
    }

    public enum Difficulty
    {
        HARDEST, HARD, MODERATE, STANDARD, EASIEST
    }

    public enum StartingResources
    {
        STANDARD, LOW, MEDIUM, HIGH
    }

    public enum RevealMap
    {
        NORMAL, EXPLORED, ALL_VISIBLE
    }

    public enum StartingAge
    {
        STANDARD, DARK_AGE = 2, FEUDAL_AGE, CASTLE_AGE, IMPERIAL_AGE, POST_IMPERIAL_AGE
    }

    public enum VictoryType
    {
        STANDARD, CONQUEST, RELICS = 4, TIME_LIMIT = 7, SCORE
    }
}
