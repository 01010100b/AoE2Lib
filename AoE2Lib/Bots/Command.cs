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
        public bool HasMessages => Messages.Count > 0;
        public bool HasResponses => Responses.Count > 0;

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
