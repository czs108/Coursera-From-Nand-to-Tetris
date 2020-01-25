using System.Security.Permissions;
using System.Runtime;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using System;

namespace Project6
{
    public static class Assembler
    {
        private const int BITS = 16;

        public static void Assemble(string inputfile, string outputfile)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(inputfile));
            Debug.Assert(!String.IsNullOrWhiteSpace(outputfile));

            SymbolTable symTab = ScanLabel(inputfile);
            ScanSymbol(inputfile, symTab);

            Parser parser = new Parser(inputfile);
            StreamWriter sw = new StreamWriter(outputfile);
            while (parser.HasMoreCommands())
            {
                parser.Advance();
                string binary = null;
                if (parser.CommandType() == CmdType.A)
                {
                    binary = FormatCommandA(parser, symTab);
                }
                else if (parser.CommandType() == CmdType.C)
                {
                    binary = FormatCommandC(parser);
                }

                if (binary != null)
                {
                    sw.WriteLine(binary);
                }
            }

            sw.Close();
        }

        private static SymbolTable ScanLabel(string inputfile)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(inputfile));

            int lineNumber = -1;
            Parser parser = new Parser(inputfile);
            SymbolTable symTab = new SymbolTable();
            while (parser.HasMoreCommands())
            {
                parser.Advance();
                if (parser.CommandType()  == CmdType.A
                    || parser.CommandType()  == CmdType.C)
                {
                    ++lineNumber;
                }

                if (parser.CommandType() == CmdType.Label)
                {
                    string label = parser.Symbol();
                    Debug.Assert(!symTab.Contains(label));
                    symTab.AddEntry(label, lineNumber + 1);
                }
            }

            return symTab;
        }

        private static void ScanSymbol(string inputfile, SymbolTable symTab)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(inputfile));
            Debug.Assert(symTab != null);

            int lineNumber = -1;
            Memory memory = new Memory();
            Parser parser = new Parser(inputfile);
            while (parser.HasMoreCommands())
            {
                parser.Advance();
                if (parser.CommandType() != CmdType.Whitespace)
                {
                    ++lineNumber;
                }

                if (parser.CommandType() == CmdType.A)
                {
                    string label = parser.Symbol();
                    if (!IsNumber(label) && !symTab.Contains(label))
                    {
                        symTab.AddEntry(label, memory.Allocate());
                    }
                }
            }
        }

        private static bool IsNumber(string label)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(label));

            Regex rx = new Regex(@"^\d+$");
            return rx.IsMatch(label);
        }

        private static string FormatCommandA(Parser parser, SymbolTable symTab)
        {
            Debug.Assert(symTab != null);
            Debug.Assert(parser != null && parser.CommandType() == CmdType.A);

            string sym = parser.Symbol();
            int value = 0;
            if (!IsNumber(sym))
            {
                value = symTab.GetAddress(sym);
            }
            else
            {
                value = Convert.ToInt32(sym);
            }

            return String.Format("0{0}", Convert.ToString(value, 2).PadLeft(BITS - 1, '0'));
        }

        private static string FormatCommandC(Parser parser)
        {
            Debug.Assert(parser != null && parser.CommandType() == CmdType.C);

            string comp = Code.Comp(parser.Comp());
            string dest = Code.Dest(parser.Dest());
            string jump = Code.Jump(parser.Jump());

            return String.Format("111{0}{1}{2}", comp, dest, jump);
        }

        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine(" [!] ERROR: There is no input file.");
                return;
            }

            try
            {
                string input = args[0];
                string output = String.Format(@"{0}\{1}{2}", Path.GetDirectoryName(input),
                    Path.GetFileNameWithoutExtension(input), ".hack");
                Assembler.Assemble(input, output);
                Console.WriteLine(" [*] The translation has finished.");
            }
            catch (Exception e)
            {
                Console.WriteLine(" [!] ERROR: " + e.ToString());
            }
        }
    }
}
