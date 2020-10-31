using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary
{
    public abstract class Module
    {
        internal abstract IEnumerable<Command> RequestUpdate(Bot bot);
        internal abstract void Update(Bot bot);

    }
}
