using Unary.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using Google.Protobuf;

namespace Unary.GameElements
{
    public class Tile : GameElement
    {
        public readonly Position Position;
        public int Elevation { get; private set; } = -1;
        public int TerrainId { get; private set; } = -1;
        public bool Explored { get; private set; } = false;
        public IReadOnlyCollection<Unit> Units => _Units;
        internal readonly HashSet<Unit> _Units = new HashSet<Unit>();

        public Tile(Position position) : base()
        {
            Position = position;
        }

        protected override void UpdateElement(IEnumerable<IMessage> responses)
        {
            throw new NotImplementedException();
        }
    }
}
