using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Protos.Expert;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        public void Add(IMessage command)
        {
            Messages.Add(command);
        }

        public void Add(params IMessage[] commands)
        {
            Messages.AddRange(commands);
        }

        public void Add(IMessage fact, string op, int value, IMessage command)
        {
            var cc = new ConditionalCommand()
            {
                Fact = Any.Pack(fact),
                CompareOp = op,
                Value = value,
                Command = Any.Pack(command)
            };

            Messages.Add(cc);
        }

        public void Add(IMessage fact, string op, int value, params IMessage[] commands)
        {
            foreach (var command in commands)
            {
                var cc = new ConditionalCommand()
                {
                    Fact = Any.Pack(fact),
                    CompareOp = op,
                    Value = value,
                    Command = Any.Pack(command)
                };

                Messages.Add(cc);
            }
        }

        public IReadOnlyList<Any> GetResponses()
        {
            if (Responses.Count != Messages.Count)
            {
                Reset();
            }

            return Responses;
        }
    }
}
