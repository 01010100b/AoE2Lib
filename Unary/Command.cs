using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary
{
    class Command
    {
        public readonly List<IMessage> Messages = new List<IMessage>();
        public readonly List<Any> Responses = new List<Any>();
    }
}
