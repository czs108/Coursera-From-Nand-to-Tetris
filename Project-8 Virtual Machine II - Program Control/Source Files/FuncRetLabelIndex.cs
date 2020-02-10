using System.Diagnostics;
using System.Collections.Generic;
using System;

namespace Project8
{
    public class FuncRetLabelIndex
    {
        private Dictionary<string, UniqueLabelIndex> map =
            new Dictionary<string, UniqueLabelIndex>();

        public int Next(string function)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(function));

            if (!map.ContainsKey(function))
            {
                map.Add(function, new UniqueLabelIndex());
            }

            return map[function].Next();
        }
    }
}