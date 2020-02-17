using System.Diagnostics;
using System.IO;
using System;

namespace Project8
{
    public class VMTranslator
    {
        private const string SYSTEM_INIT = "Sys.vm";

        public void TranslateFile(string inputFile, string outputFile)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(inputFile));
            Debug.Assert(!String.IsNullOrWhiteSpace(outputFile));

            using (CodeWriter writer = new CodeWriter(outputFile))
            {
                TranslateFile(inputFile, writer);
            }
        }

        public void TranslateDirectory(string inputDir, string outputFile)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(inputDir));
            Debug.Assert(!String.IsNullOrWhiteSpace(outputFile));

            using (CodeWriter writer = new CodeWriter(outputFile))
            {
                if (File.Exists(Path.Combine(inputDir, SYSTEM_INIT)))
                {
                    writer.WriteInit();
                }

                foreach (string inputFile in Directory.GetFiles(inputDir, "*.vm"))
                {
                    TranslateFile(inputFile, writer);
                }
            }
        }

        private void TranslateFile(string inputFile, CodeWriter writer)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(inputFile));
            Debug.Assert(writer != null);

            using (Parser parser = new Parser(inputFile))
            {
                writer.SetFileName(inputFile);

                while (parser.HasMoreCommands())
                {
                    parser.Advance();
                    switch (parser.TypeOfCommand())
                    {
                        case CommandType.Arithmetic:
                        {
                            writer.WriteArithmetic(parser.Arg1());
                            break;
                        }
                        case CommandType.Push:
                        {
                            writer.WritePushPop("push", parser.Arg1(), parser.Arg2());
                            break;
                        }
                        case CommandType.Pop:
                        {
                            writer.WritePushPop("pop", parser.Arg1(), parser.Arg2());
                            break;
                        }
                        case CommandType.Label:
                        {
                            writer.WriteLabel(parser.Arg1());
                            break;
                        }
                        case CommandType.Goto:
                        {
                            writer.WriteGoto(parser.Arg1());
                            break;
                        }
                        case CommandType.If:
                        {
                            writer.WriteIf(parser.Arg1());
                            break;
                        }
                        case CommandType.Function:
                        {
                            writer.WriteFunction(parser.Arg1(), parser.Arg2());
                            break;
                        }
                        case CommandType.Call:
                        {
                            writer.WriteCall(parser.Arg1(), parser.Arg2());
                            break;
                        }
                        case CommandType.Return:
                        {
                            writer.WriteReturn();
                            break;
                        }
                        default:
                        {
                            break;
                        }
                    }
                }
            }
        }

        private static string GetOutputName(string inputName, out bool isDirectory)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(inputName));

            if (String.IsNullOrEmpty(Path.GetExtension(inputName)))
            {
                isDirectory = true;
                return Path.Combine(inputName,
                    $"{Path.GetFileNameWithoutExtension(inputName)}.asm");
            }
            else
            {
                isDirectory = false;
                return Path.Combine(Path.GetDirectoryName(inputName),
                    $"{Path.GetFileNameWithoutExtension(inputName)}.asm");
            }
        }

        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine(" [!] ERROR: There is no input file or directory.");
                return;
            }

            try
            {
                string input = args[0];
                string output = GetOutputName(input, out bool isDirectory);
                if (isDirectory)
                {
                    new VMTranslator().TranslateDirectory(input, output);
                }
                else
                {
                    new VMTranslator().TranslateFile(input, output);
                }

                Console.WriteLine(" [*] The translation has finished.");
            }
            catch (Exception e)
            {
                Console.WriteLine(" [!] ERROR: " + e.ToString());
            }
        }
    }
}
