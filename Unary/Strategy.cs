using AoE2Lib;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.UnitControllers;
using Unary.UnitControllers.VillagerControllers;

namespace Unary
{
    class Strategy
    {
        public class BuildOrderCommand
        {
            public enum BuildOrderCommandType { RESEARCH, UNIT }

            public BuildOrderCommandType Type { get; set; }
            public int Id { get; set; }
        }

        public static Strategy Deserialize(string str)
        {
            var serializer = new JsonSerializer
            {
                Formatting = Formatting.Indented
            };
            serializer.Converters.Add(new StringEnumConverter());

            using (var reader = new StringReader(str))
            using (var json = new JsonTextReader(reader))
            {
                return serializer.Deserialize<Strategy>(json);
            }
        }

        public static string Serialize(Strategy strategy)
        {
            var serializer = new JsonSerializer
            {
                Formatting = Formatting.Indented
            };
            serializer.Converters.Add(new StringEnumConverter());

            using (var writer = new StringWriter())
            using (var json = new JsonTextWriter(writer))
            {
                serializer.Serialize(json, strategy);

                return writer.ToString();
            }
        }

        public string Name { get; set; } = "";
        public List<Resource> Gatherers { get; set; } = new();
        public int ExtraFoodPercentage { get; set; } = 0;
        public int ExtraWoodPercentage { get; set; } = 0;
        public int ExtraGoldPercentage { get; set; } = 0;
        public int ExtraStonePercentage { get; set; } = 0;
        public List<BuildOrderCommand> BuildOrder { get; set; } = new();
        public List<int> PrimaryUnits { get; set; } = new();
        public List<int> SecondaryUnits { get; set; } = new();
        public int SecondaryUnitPercentage { get; set; } = 0;

        private Unary Unary { get; set; }

        public int GetCurrentGatherers(Resource resource)
        {
            var gatherers = 0;

            foreach (var ctrl in Unary.UnitsManager.GetControllers<UnitController>())
            {
                if (ctrl is GathererController gatherer)
                {
                    if (gatherer.Resource == resource)
                    {
                        gatherers++;
                    }
                }
                else if (resource == Resource.FOOD)
                {
                    if (ctrl is FarmerController || ctrl is HunterController)
                    {
                        gatherers++;
                    }
                }
            }

            return gatherers;
        }

        public int GetMinimumGatherers(Resource resource)
        {
            return (int)Math.Ceiling(GetDesiredGatherers(resource) * 0.9);
        }

        public int GetMaximumGatherers(Resource resource)
        {
            return (int)Math.Floor(GetDesiredGatherers(resource) * 1.1);
        }

        internal void SetUnary(Unary unary)
        {
            Unary = unary;
        }

        private int GetDesiredGatherers(Resource resource)
        {
            var gatherers = 0;
            var pop = Unary.GameState.MyPlayer.CivilianPopulation;

            for (int i = 0; i < Math.Min(pop, Gatherers.Count); i++)
            {
                if (Gatherers[i] == resource)
                {
                    gatherers++;
                }
            }

            if (pop > Gatherers.Count)
            {
                pop -= Gatherers.Count;
                var fraction = 0d;

                switch (resource)
                {
                    case Resource.FOOD: fraction = ExtraFoodPercentage / 100d; break;
                    case Resource.WOOD: fraction = ExtraWoodPercentage / 100d; break;
                    case Resource.GOLD: fraction = ExtraGoldPercentage / 100d; break;
                    case Resource.STONE: fraction = ExtraStonePercentage / 100d; break;
                }

                gatherers += (int)Math.Round(pop * fraction);
            }

            return gatherers;
            throw new NotImplementedException();
        }
    }
}
