using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace AoE2Lib.Bots
{
    public class Command
    {
        public readonly List<IMessage> Messages = new List<IMessage>();
        public readonly List<Any> Responses = new List<Any>();
    }
}
