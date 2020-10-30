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
        public readonly Command Command = new Command();

        protected internal abstract void RequestUpdate();
        protected internal abstract void Update();

    }
}
