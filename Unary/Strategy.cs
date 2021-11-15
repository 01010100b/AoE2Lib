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
        public bool AutoEcoTechs { get; set; } = false;
    }
}
