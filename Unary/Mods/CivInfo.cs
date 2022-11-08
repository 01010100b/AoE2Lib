using AoE2Lib;
using AoE2Lib.Bots;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YTY.AocDatLib;

namespace Unary.Mods
{
    internal class CivInfo
    {
        public readonly int Id;
        public readonly HashSet<int> AvailableUnits = new();
        public readonly HashSet<int> AvailableTechs = new();
        public int FarmId { get; } = 50;
        public int TownCenterId { get; } = 109;

        private readonly Dictionary<int, DatUnit> AllUnits = new();
        private readonly Mod Mod;
        private readonly DatFile Dat;

        internal CivInfo(Mod mod, DatFile dat, int id)
        {
            Mod = mod;
            Dat = dat;
            Id = id;

            Load();
        }

        public string GetUnitName(int unit) => Mod.GetString(AllUnits[unit].Name);
        public Position GetUnitCollisionSize(int unit) => new Position(AllUnits[unit].CollisionSizeX, AllUnits[unit].CollisionSizeY);
        public int GetUnitWidth(int unit) => Math.Max(1, (int)Math.Round(AllUnits[unit].CollisionSizeX * 2));
        public int GetUnitHeight(int unit) => Math.Max(1, (int)Math.Round(AllUnits[unit].CollisionSizeY * 2));
        public int GetUnitHillMode(int unit) => AllUnits[unit].HillMode;

        public IEnumerable<Resource> GetDropsiteResources(int unit)
        {
            if (unit == TownCenterId)
            {
                yield return Resource.WOOD;
                yield return Resource.FOOD;
                yield return Resource.GOLD;
                yield return Resource.STONE;
            }
            else if (unit == Mod.Mill) // mill
            {
                yield return Resource.FOOD;
            }
            else if (unit == Mod.LumberCamp)
            {
                yield return Resource.WOOD;
            }
            else if (unit == Mod.GoldMiningCamp)
            {
                yield return Resource.GOLD;
                yield return Resource.STONE;
            }
        }

        public IEnumerable<KeyValuePair<int, int>> GetUnitTechEffectCounts(int unit)
        {
            var counts = ObjectPool.Get(() => new Dictionary<int, int>(), x => x.Clear());

            foreach (var kvp in Mod.GetUnitEffects(unit))
            {
                var tech = kvp.Key;
                var effect = kvp.Value;

                if (AvailableTechs.Contains(tech))
                {
                    if (!counts.ContainsKey(tech))
                    {
                        counts.Add(tech, 0);
                    }

                    switch (effect.Command)
                    {
                        case 2:
                        case 12: break; // enable/disable
                        case 3:
                        case 13: counts[tech] += 3; break; // upgrade
                        default: counts[tech] += 1; break;
                    }
                }
            }

            foreach (var kvp in counts.Where(x => x.Value > 0))
            {
                yield return kvp;
            }

            ObjectPool.Add(counts);
        }

        public bool BlocksPassage(int unit)
        {
            var def = AllUnits[unit];

            if (def.Speed > 0)
            {
                return false;
            }

            return (int)def.ObstructionType switch
            {
                2 or 3 or 5 or 10 => true,
                _ => false,
            };
        }

        public bool CanPassTerrain(int unit, int terrain)
        {
            var table = AllUnits[unit].TerrainRestriction;

            return Mod.IsTerrainPassable(table, terrain);
        }

        private void Load()
        {
            var civ = Dat.Civilizations[Id];

            foreach (var unit in civ.Units)
            {
                AllUnits.Add(unit.Id, unit);
            }

            // available techs

            var disabled = new HashSet<int>();
            var tech = Dat.Technologies[civ.TechTreeId];

            foreach (var effect in tech.Effects)
            {
                if (effect.Command == 102) // disable tech
                {
                    disabled.Add((int)effect.Arg4);
                }
            }

            for (int i = 0; i < Dat.Researches.Count; i++)
            {
                var research = Dat.Researches[i];

                if (research.Civilization < 0 || research.Civilization == Id)
                {
                    if (!disabled.Contains(i))
                    {
                        AvailableTechs.Add(i);
                    }
                }
            }

            var count = -1;

            while (count != disabled.Count)
            {
                count = disabled.Count;

                foreach (var research in AvailableTechs.Select(x => Dat.Researches[x]))
                {
                    if (research.EffectId >= 0)
                    {
                        foreach (var effect in Dat.Technologies[research.EffectId].Effects)
                        {
                            if (effect.Command == 102) // disable tech
                            {
                                disabled.Add((int)effect.Arg4);
                            }
                        }
                    }
                }

                AvailableTechs.RemoveWhere(x => disabled.Contains(x));
            }

            // available units

            count = -1;

            while (count != AvailableUnits.Count)
            {
                count = AvailableUnits.Count;

                foreach (var unit in AllUnits.Values)
                {
                    if (unit.InterfaceKind == 0 || unit.TrainLocationId < 0)
                    {
                        continue;
                    }

                    if (unit.Enabled == 1)
                    {
                        AvailableUnits.Add(unit.Id);
                    }
                    else
                    {
                        foreach (var kvp in Mod.GetUnitEffects(unit.Id))
                        {
                            if (AvailableTechs.Contains(kvp.Key))
                            {
                                var effect = kvp.Value;

                                if (effect.Command == 2 || effect.Command == 12) // enable
                                {
                                    if (effect.Arg2 > 0)
                                    {
                                        AvailableUnits.Add(unit.Id);
                                        if (Id == 7 && unit.Id == 762)
                                        {
                                            Program.Log.Info($"Found 762 tech {kvp.Key}");
                                        }
                                    }
                                }
                                else if (effect.Command == 3 || effect.Command == 13) // upgrade
                                {
                                    if (unit.Id == effect.Arg1 && AvailableUnits.Contains(unit.Id))
                                    {
                                        AvailableUnits.Add(effect.Arg2);
                                        if (Id == 7 && effect.Arg2 == 762)
                                        {
                                            Program.Log.Info($"Found 762 tech {kvp.Key}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
