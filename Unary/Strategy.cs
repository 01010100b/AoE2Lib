using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unary.GameElements;
using Unary.Utils;

namespace Unary
{
    public abstract class Strategy
    {
        public abstract void Update(Bot bot);
        internal void UpdateInternal(Bot bot)
        {
            Update(bot);
        }
    }
}
