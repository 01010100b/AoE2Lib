using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary
{
    abstract class Module
    {
        protected internal abstract IEnumerable<Command> RequestUpdate(Bot bot);
        protected internal abstract void Update();

    }
}
