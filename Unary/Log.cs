using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unary
{
    static class Log
    {
        public static void Debug(object message)
        {
            System.Diagnostics.Debug.WriteLine(message.ToString());
        }
    }
}
