using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary.Mods
{
    public class UnitDef
    {
        public int Id { get; set; }
        public int FoundationId { get; set;}
        public int Width { get; set; } = 1;
        public int Height { get; set; } = 1;
        public UnitClass Class { get; set; }
    }
}
