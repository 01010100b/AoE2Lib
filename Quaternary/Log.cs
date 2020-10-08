using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaternary
{
    static class Log
    {
        public static void Debug(object message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
    }
}
