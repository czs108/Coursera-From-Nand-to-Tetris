using System.Diagnostics;
using System.IO;
using System;

namespace Project7
{
    public class VMTranslator
    {
        public void TranslateFile(string inputFile, string outputFile)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(inputFile));
            Debug.Assert(!String.IsNullOrWhiteSpace(outputFile));

            using (CodeWriter writer = new CodeWriter(outputFile))
            {
                TranslateFile(inputFile, writer);
            }
        }

        private void TranslateFile(string inputFile, CodeWriter writer)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(inputFile));
            Debug.Assert(writer != null);

            using (Parser parser = new Parser(inputFile))
            {
                while (parser.HasMoreCommands())
                {
                    parser.Advance();
                    switch (parser.CommandType())
                    {
                        case CmdType.Arithmetic:
                        {
                            writer.WriteArithmetic(parser.Arg1());
                            break;
                        }
                        case CmdType.Push:
                        {
                            writer.WritePushPop("push", parser.Arg1(), parser.Arg2());
                            break;
                        }
                        case CmdType.Pop:
                        {
                            writer.WritePushPop("pop", parser.Arg1(), parser.Arg2());
                            break;
                        }
                        default:
                        {
                            // TODO...
                            break;
                        }
                    }
                }
            }
        }

        private static string GetOutputName(string inputName)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(inputName));

            return Path.Combine(Path.GetDirectoryName(inputName),
                $"{Path.GetFileNameWithoutExtension(inputName)}.asm");
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
                new VMTranslator().TranslateFile(input, output);
                Console.WriteLine(" [*] The translation has finished.");
            }
            catch (Exception e)
            {
                Console.WriteLine(" [!] ERROR: " + e.ToString());
            }
        }
    }
}
