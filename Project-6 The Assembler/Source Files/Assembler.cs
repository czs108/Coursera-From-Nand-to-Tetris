using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using System;

namespace Project6
{
    public class Assembler
    {
        private const int BITS = 16;

        private SymbolTable symTab = default;

        public void Assemble(string inputFile, string outputFile)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(inputFile));
            Debug.Assert(!String.IsNullOrWhiteSpace(outputFile));

            symTab = new SymbolTable();
            ScanLabel(inputFile);
            ScanSymbol(inputFile);

            using (Parser parser = new Parser(inputFile))
            {
                using (CodeWriter writer = new CodeWriter(outputFile, BITS))
                {
                    while (parser.HasMoreCommands())
                    {
                        parser.Advance();
                        if (parser.CommandType() == CmdType.A)
                        {
                            int value = GetValue(parser.Symbol());
                            writer.WriteCommandA(value);
                        }
                        else if (parser.CommandType() == CmdType.C)
                        {
                            writer.WriteCommandC(parser.Comp(), parser.Dest(), parser.Jump());
                        }
                    }
                }
            }
        }

        private void ScanLabel(string inputFile)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(inputFile));

            int lineNumber = -1;
            using (Parser parser = new Parser(inputFile))
            {
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
            }
        }

        private void ScanSymbol(string inputFile)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(inputFile));

            int lineNumber = -1;
            Memory memory = new Memory();
            using (Parser parser = new Parser(inputFile))
            {
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
        }

        private int GetValue(string symbol)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(symbol));

            int value = default;
            if (!IsNumber(symbol))
            {
                value = symTab.GetAddress(symbol);
            }
            else
            {
                value = Convert.ToInt32(symbol);
            }

            return value;
        }

        private static bool IsNumber(string label)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(label));

            Regex rx = new Regex(@"^\d+$");
            return rx.IsMatch(label);
        }

        private static string GetOutputName(string inputName)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(inputName));

            return Path.Combine(Path.GetDirectoryName(inputName),
                $"{Path.GetFileNameWithoutExtension(inputName)}.hack");
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
                string output = GetOutputName(input);
                new Assembler().Assemble(input, output);
                Console.WriteLine(" [*] The translation has finished.");
            }
            catch (Exception e)
            {
                Console.WriteLine(" [!] ERROR: " + e.ToString());
            }
        }
    }
}
