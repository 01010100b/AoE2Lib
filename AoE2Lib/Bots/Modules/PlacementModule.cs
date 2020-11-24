﻿using AoE2Lib.Bots.GameElements;
using AoE2Lib.Mods;
using AoE2Lib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AoE2Lib.Bots.Modules
{
    public class PlacementModule : Module
    {
        public static bool IsUnitClassObstruction(UnitClass unitclass)
        {
            switch (unitclass)
            {
                case UnitClass.Artifact:
                case UnitClass.BerryBush:
                case UnitClass.Building:
                case UnitClass.Cliff:
                case UnitClass.DeepSeaFish:
                case UnitClass.Farm:
                case UnitClass.Flag:
                case UnitClass.Gate:
                case UnitClass.GoldMine:
                case UnitClass.MiscBuilding:
                case UnitClass.OceanFish:
                case UnitClass.OreMine:
                case UnitClass.ResourcePile:
                case UnitClass.SalvagePile:
                case UnitClass.ShoreFish:
                case UnitClass.StoneMine:
                case UnitClass.Tower:
                case UnitClass.Tree:
                case UnitClass.Wall:
                    return true;
                default:
                    return false;
            }
        }

        public static IEnumerable<Position> GetFootprint(Position position, int width, int height, int clearance)
        {
            var xmin = position.PointX - (width / 2) - clearance;
            var xmax = position.PointX + ((width - 1) / 2) + clearance;
            var ymin = position.PointY - (height / 2) - clearance;
            var ymax = position.PointY + ((height - 1) / 2) + clearance;

            for (int x = xmin; x <= xmax; x++)
            {
                for (int y = ymin; y <= ymax; y++)
                {
                    yield return Position.FromPoint(x, y);
                }
            }
        }

        public int FarmLayersAroundTownCenter { get; set; } = 1;
        public int FarmLayersAroundMill { get; set; } = 1;

        private readonly HashSet<Position> Restrictions = new HashSet<Position>();
        private readonly HashSet<Position> ExtraRestrictions = new HashSet<Position>();

        public IEnumerable<Position> GetPlacementPositions(UnitDef unit, Position position, int clearance, bool restricted, double range)
        {
            var map = Bot.GetModule<MapModule>();
            foreach (var tile in map.GetTilesInRange(position, range))
            {
                if (CanBuildAtPosition(map, unit, tile.Position, clearance, restricted))
                {
                    yield return tile.Position;
                }
            }
        }

        public bool CanBuildAtPosition(MapModule map, UnitDef unit, Position position, int clearance, bool restricted)
        {
            var elevation = int.MinValue;
            foreach (var pos in GetFootprint(position, unit.Width, unit.Height, clearance))
            {
                if (!map.IsOnMap(pos))
                {
                    return false;
                }

                if (Restrictions.Contains(pos))
                {
                    return false;
                }

                if (restricted && ExtraRestrictions.Contains(pos))
                {
                    return false;
                }

                var tile = map.GetTile(pos);

                if (!tile.Explored)
                {
                    return false;
                }

                if (elevation == int.MinValue)
                {
                    elevation = tile.Elevation;
                }

                // TODO hill-mode
                // TODO clearance doesn't require constant elevation
                if (elevation != tile.Elevation)
                {
                    return false;
                }

                // TODO terrain placement
                if (!tile.IsOnLand)
                {
                    return false;
                }
            }

            return true;
        }

        protected override IEnumerable<Command> RequestUpdate()
        {
            return Enumerable.Empty<Command>();
        }

        protected override void Update()
        {
            Restrictions.Clear();
            ExtraRestrictions.Clear();

            var farm = Math.Max(Bot.Mod.Farm.Width, Bot.Mod.Farm.Height);

            var units = Bot.GetModule<UnitsModule>().Units.Values;
            foreach (var unit in units.Where(u => IsUnitClassObstruction(u.Class)))
            {
                if (!unit.Exists)
                {
                    continue;
                }

                var def = Bot.Mod.Villager;
                if (Bot.Mod.UnitDefs.ContainsKey(unit.BaseTypeId))
                {
                    def = Bot.Mod.UnitDefs[unit.BaseTypeId];
                }

                foreach (var pos in GetFootprint(unit.Position, def.Width, def.Height, 0))
                {
                    Restrictions.Add(pos);
                }

                if (def == Bot.Mod.TownCenter)
                {
                    foreach (var pos in GetFootprint(unit.Position, def.Width, def.Height, farm * FarmLayersAroundTownCenter))
                    {
                        ExtraRestrictions.Add(pos);
                    }
                }
                else if (def == Bot.Mod.Mill)
                {
                    foreach (var pos in GetFootprint(unit.Position, def.Width, def.Height, farm * FarmLayersAroundMill))
                    {
                        ExtraRestrictions.Add(pos);
                    }
                }
            }
        }
    }
}
