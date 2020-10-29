using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.GameElements;
using Unary.Utils;
using static Unary.GameElements.UnitTypeInfo;

namespace Unary
{
    abstract class Strategy
    {
        public abstract void Update(Bot bot);
    }
}
