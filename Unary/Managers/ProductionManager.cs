﻿using AoE2Lib;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.UnitControllers;

namespace Unary.Managers
{
    class ProductionManager : Manager
    {
        private FarmerController FarmRequest { get; set; }
        private GathererController DropsiteRequest { get; set; }
        private readonly Dictionary<Resource, bool> AddDropsite = new();

        public ProductionManager(Unary unary) : base(unary)
        {
            AddDropsite.Add(Resource.FOOD, false);
            AddDropsite.Add(Resource.WOOD, false);
            AddDropsite.Add(Resource.GOLD, false);
            AddDropsite.Add(Resource.STONE, false);
        }

        public void RequestFarm(FarmerController controller)
        {
            if (FarmRequest == null || controller.Unit.FirstUpdateGameTime < FarmRequest.Unit.FirstUpdateGameTime)
            {
                FarmRequest = controller;
            }
        }

        public void RequestDropsite(GathererController controller)
        {
            if (DropsiteRequest == null)
            {
                DropsiteRequest = controller;
            }

            if (controller.Resource == Resource.WOOD && DropsiteRequest.Resource != Resource.WOOD)
            {
                DropsiteRequest = controller;
            }

            if (controller.Unit.FirstUpdateGameTime < DropsiteRequest.Unit.FirstUpdateGameTime)
            {
                DropsiteRequest = controller;
            }
        }

        internal override void Update()
        {
            PerformFarmRequest();
            PerformDropsiteRequest();
            BuildDropsites();
        }

        private void PerformFarmRequest()
        {
            if (FarmRequest == null)
            {
                return;
            }

            var farm = Unary.GameState.GetUnitType(Unary.Mod.Farm);
            var mill = Unary.GameState.GetUnitType(Unary.Mod.Mill);

            if (FarmRequest.Tile == null)
            {
                var placements = Unary.BuildingManager.GetFarmPlacements();

                if (placements.Count > 0)
                {
                    Unary.Log.Info("Building new farm");
                    farm.BuildLine(placements, 100, 3, Priority.FARM);
                }

                if (placements.Count <= 3)
                {
                    Unary.Log.Info("Building mill");
                    mill.BuildNormal(100, 1, Priority.FARM + 1);
                }
            }
            else
            {
                if (Unary.BuildingManager.CanBuildAt(farm, FarmRequest.Tile, true))
                {
                    Unary.Log.Info($"Refreshing farm at {FarmRequest.Tile.Position}");
                    farm.BuildLine(new[] { FarmRequest.Tile }, 100, 3, Priority.FARM);
                }
                else
                {
                    Unary.Log.Debug($"Can not build farm at {FarmRequest.Tile.Position}");
                }
            }

            FarmRequest = null;
        }

        private void PerformDropsiteRequest()
        {
            if (DropsiteRequest == null)
            {
                return;
            }

            Unary.Log.Debug($"Performing dropsite request {DropsiteRequest.Resource}");

            var site = Unary.GameState.GetUnitType(Unary.Mod.LumberCamp);
            switch (DropsiteRequest.Resource)
            {
                case Resource.FOOD: site = Unary.GameState.GetUnitType(Unary.Mod.Mill); break;
                case Resource.WOOD: site = Unary.GameState.GetUnitType(Unary.Mod.LumberCamp); break;
                case Resource.GOLD: site = Unary.GameState.GetUnitType(Unary.Mod.MiningCamp); break;
                case Resource.STONE: site = Unary.GameState.GetUnitType(Unary.Mod.MiningCamp); break;
            }

            if (site.Updated && site.Pending == 0)
            {
                AddDropsite[DropsiteRequest.Resource] = true;
            }

            DropsiteRequest = null;
        }

        private void BuildDropsites()
        {
            foreach (var resource in new[] { Resource.WOOD, Resource.FOOD, Resource.GOLD, Resource.STONE })
            {
                var site = Unary.GameState.GetUnitType(Unary.Mod.LumberCamp);

                switch (resource)
                {
                    case Resource.FOOD: site = Unary.GameState.GetUnitType(Unary.Mod.Mill); break;
                    case Resource.WOOD: site = Unary.GameState.GetUnitType(Unary.Mod.LumberCamp); break;
                    case Resource.GOLD: site = Unary.GameState.GetUnitType(Unary.Mod.MiningCamp); break;
                    case Resource.STONE: site = Unary.GameState.GetUnitType(Unary.Mod.MiningCamp); break;
                }

                if (site.Updated && site.Pending > 0)
                {
                    AddDropsite[resource] = false;

                    continue;
                }

                if (site.Updated && site.Pending == 0 && AddDropsite[resource])
                {
                    var placements = Unary.BuildingManager.GetDropsitePlacements(resource);
                    placements.Sort((a, b) => b.Value.CompareTo(a.Value));
                    site.BuildLine(placements.Select(p => p.Key), 100, 1, Priority.DROPSITE);
                    Unary.Log.Debug($"Found {placements.Count} placements for {resource}");

                    continue;
                }
            }
        }
    }
}
