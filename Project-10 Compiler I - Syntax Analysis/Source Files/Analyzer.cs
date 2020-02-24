using System.Diagnostics;
using System.IO;
using System;

namespace Project10
{
    public class Analyzer
    {
        public void AnalyzeFile(string inputFile, string outputFile)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(inputFile));
            Debug.Assert(!String.IsNullOrWhiteSpace(outputFile));

            using (Tokenizer tokens = new Tokenizer(inputFile))
            {
                new CompilationEngine().Compile(tokens, outputFile);
            }
        }

        public void AnalyzeDirectory(string inputDir, string outputDir)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(inputDir));
            Debug.Assert(!String.IsNullOrWhiteSpace(outputDir));

            foreach (string inputFile in Directory.GetFiles(inputDir, "*.jack"))
            {
                string outputFile = Path.Combine(outputDir, $"{Path.GetFileNameWithoutExtension(inputFile)}.xml");
                AnalyzeFile(inputFile, outputFile);
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
                    $"{Path.GetFileNameWithoutExtension(inputName)}.xml");
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
                    new Analyzer().AnalyzeDirectory(input, output);
                }
                else
                {
                    new Analyzer().AnalyzeFile(input, output);
                }

                Console.WriteLine(" [*] The analysis has finished.");
            }
            catch (Exception e)
            {
                Console.WriteLine(" [!] ERROR: " + e.ToString());
            }
        }
    }
}
