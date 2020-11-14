using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AoE2Lib.Bots
{
    public class Command
    {
        internal readonly List<IMessage> Messages = new List<IMessage>();
        internal readonly List<Any> Responses = new List<Any>();

        public void Reset()
        {
            Messages.Clear();
            Responses.Clear();
        }

        public void Add(IMessage message)
        {
            Messages.Add(message);
        }

        public IReadOnlyList<Any> GetResponses()
        {
            Debug.Assert(Messages.Count == Responses.Count);

            return Responses;
        }
    }
}
