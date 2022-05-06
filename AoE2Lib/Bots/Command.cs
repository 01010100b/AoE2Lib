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
                InConstValue = value,
                Command = Any.Pack(command)
            };

            Messages.Add(cc);
        }

        public void Add(IMessage fact, string op, int value, params IMessage[] messages)
        {
            foreach (var message in messages)
            {
                var m = new ConditionalCommand()
                {
                    Fact = Any.Pack(fact),
                    CompareOp = op,
                    InConstValue = value,
                    Command = Any.Pack(message)
                };

                Messages.Add(m);
            }
        }

        public void Add(IMessage fact, string op, int value, Command command)
        {
            Add(fact, op, value, command.Messages.ToArray());
        }

        public IReadOnlyList<Any> GetResponses()
        {
            return Responses;
        }
    }
}
