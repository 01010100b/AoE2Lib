using System;
using System.Collections.Generic;
using System.Text;

namespace Unary.Learning
{
    public class Solution
    {
        public double Fitness { get; set; } = double.NaN;
        public readonly List<float> Parameters = new List<float>();
    }
}
