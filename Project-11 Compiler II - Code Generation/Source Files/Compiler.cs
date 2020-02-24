using System.Diagnostics;
using System.IO;
using System;

namespace Project11
{
    public class Compiler
    {
        public void CompileFile(string inputFile, string outputFile)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(inputFile));
            Debug.Assert(!String.IsNullOrWhiteSpace(outputFile));

            using (Tokenizer tokens = new Tokenizer(inputFile))
            {
                using (VMWriter writer = new VMWriter(outputFile))
                {
                    new CompilationEngine().Compile(tokens, writer);
                }
            }
        }

        public void CompileDirectory(string inputDir, string outputDir)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(inputDir));
            Debug.Assert(!String.IsNullOrWhiteSpace(outputDir));

            foreach (string inputFile in Directory.GetFiles(inputDir, "*.jack"))
            {
                string outputFile = Path.Combine(outputDir, $"{Path.GetFileNameWithoutExtension(inputFile)}.vm");
                CompileFile(inputFile, outputFile);
            }
        }

        private static string GetOutputName(string inputName, out bool isDirectory)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(inputName));

            if (String.IsNullOrEmpty(Path.GetExtension(inputName)))
            {
                isDirectory = true;
                return inputName;
            }
            else
            {
                isDirectory = false;
                return Path.Combine(Path.GetDirectoryName(inputName),
                    $"{Path.GetFileNameWithoutExtension(inputName)}.vm");
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
                    new Compiler().CompileDirectory(input, output);
                }
                else
                {
                    new Compiler().CompileFile(input, output);
                }

                Console.WriteLine(" [*] The compilation has finished.");
            }
            catch (Exception e)
            {
                Console.WriteLine(" [!] ERROR: " + e.ToString());
            }
        }
    }
}
