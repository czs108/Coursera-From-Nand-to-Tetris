using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Project6
{
    public static class Code
    {
        private static readonly Dictionary<string, string> destMap =
            new Dictionary<string, string>()
        {
            { "M", "001" },
            { "D", "010" },
            { "MD", "011" },
            { "A", "100" },
            { "AM", "101" },
            { "AD", "110" },
            { "AMD", "111" }
        };

        private static readonly Dictionary<string, string> compMap =
            new Dictionary<string, string>()
        {
            { "0", "101010" },
            { "1", "111111" },
            { "-1", "111010" },
            { "D", "001100" },
            { "A", "110000" },
            { "!D", "001101" },
            { "!A", "110001" },
            { "-D", "001111" },
            { "-A", "110011" },
            { "D+1", "011111" },
            { "A+1", "110111" },
            { "D-1", "001110" },
            { "A-1", "110010" },
            { "D+A", "000010" },
            { "D-A", "010011" },
            { "A-D", "000111" },
            { "D&A", "000000" },
            { "D|A", "010101" }
        };

        private static readonly Dictionary<string, string> jmpMap =
            new Dictionary<string, string>()
        {
            { "JGT", "001" },
            { "JEQ", "010" },
            { "JGE", "011" },
            { "JLT", "100" },
            { "JNE", "101" },
            { "JLE", "110" },
            { "JMP", "111" }
        };

        public static string Dest(string mnemonic) => search(destMap, mnemonic, "000");

        public static string Comp(string mnemonic)
        {
            string binary = search(compMap, mnemonic.Replace('M', 'A'));
            if (mnemonic.Contains("M"))
            {
                return "1" + binary;
            }
            else
            {
                return "0" + binary;
            }
        }

        public static string Jump(string mnemonic) => search(jmpMap, mnemonic, "000");

        private static string search(Dictionary<string, string> map,
            string mnemonic, string defBinary = null)
        {
            Debug.Assert(map != null);

            if (!String.IsNullOrWhiteSpace(mnemonic)
                && map.TryGetValue(mnemonic, out string binary))
            {
                return binary;
            }
            else if (defBinary != null)
            {
                return defBinary;
            }
            else
            {
                throw new KeyNotFoundException(
                    " [!] The symbol is not in the map.");
            }
        }
    }
}