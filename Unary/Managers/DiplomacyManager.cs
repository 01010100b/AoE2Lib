using AoE2Lib.Bots;
using AoE2Lib.Bots.GameElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Managers
{
    class DiplomacyManager : Manager
    {
        public const int PROTOCOL_VERSION = 17231;
        
        public class UnitIntel
        {
            public readonly int Id;
            public readonly int TypeId;
            public readonly int Player;
            public readonly Position Position;
            public readonly TimeSpan GameTime;

            public UnitIntel(int id, int type_id, int player, Position position, TimeSpan gametime)
            {
                Id = id;
                TypeId = type_id;
                Player = player;
                Position = position;
                GameTime = gametime;
            }
        }

        private readonly Dictionary<int, UnitIntel> UnitIntels = new();
        private readonly HashSet<Tile> ObstructedTiles = new();

        public DiplomacyManager(Unary unary) : base(unary)
        {

        }

        public IEnumerable<UnitIntel> GetUnitIntels()
        {
            return UnitIntels.Values;
        }

        public bool IsObstructed(Tile tile)
        {
            return ObstructedTiles.Contains(tile);
        }

        internal override void Update()
        {
            for (int sn = 400; sn < 512; sn++)
            {
                Unary.GameState.SetStrategicNumber(sn, -1);
            }
        }
    }
}
