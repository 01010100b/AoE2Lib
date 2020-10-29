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
        internal abstract IEnumerable<IMessage> GetMessages(Bot bot);
    }
}
