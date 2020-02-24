using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;

namespace Project8
{
    public class Parser : IDisposable
    {
        private static readonly HashSet<string> arithmeticCmds = new HashSet<string>()
        {
            "add", "sub", "neg", "eq", "gt", "lt", "and", "or", "not"
        };

        private StreamReader inputFile = null;

        private string currentLine = null;

        private string nextLine = null;

        public Parser(string filename)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(filename));

            inputFile = new StreamReader(filename);
            nextLine = ReadLine(inputFile);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // NOTE: Leave out the finalizer altogether if this class doesn't
        // own unmanaged resources itself, but leave the other methods
        // exactly as they are.
        ~Parser()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        public bool HasMoreCommands() => !String.IsNullOrEmpty(nextLine);

        public void Advance()
        {
            Debug.Assert(HasMoreCommands());

            currentLine = nextLine;
            nextLine = ReadLine(inputFile);
        }

        public CommandType TypeOfCommand()
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(currentLine));

            string[] fields = currentLine.Split(' ');
            string cmd = fields[0];
            if (arithmeticCmds.Contains(cmd))
            {
                return CommandType.Arithmetic;
            }

            switch (cmd)
            {
                case "push":
                {
                    return CommandType.Push;
                }
                case "pop":
                {
                    return CommandType.Pop;
                }
                case "label":
                {
                    return CommandType.Label;
                }
                case "goto":
                {
                    return CommandType.Goto;
                }
                case "if-goto":
                {
                    return CommandType.If;
                }
                case "function":
                {
                    return CommandType.Function;
                }
                case "call":
                {
                    return CommandType.Call;
                }
                case "return":
                {
                    return CommandType.Return;
                }
                default:
                {
                    throw new FormatException(
                        " [!] The command is invalid.");
                }
            }
        }

        public string Arg1()
        {
            Debug.Assert(TypeOfCommand() != CommandType.Return);

            string[] fields = currentLine.Split(' ');
            if (TypeOfCommand() == CommandType.Arithmetic)
            {
                return fields[0];
            }
            else
            {
                return fields[1];
            }
        }

        public int Arg2()
        {
            Debug.Assert(TypeOfCommand() == CommandType.Push
                || TypeOfCommand() == CommandType.Pop
                || TypeOfCommand() == CommandType.Function
                || TypeOfCommand() == CommandType.Call);

            string[] fields = currentLine.Split(' ');
            return Convert.ToInt32(fields[2]);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free managed resources
                inputFile?.Close();
                inputFile = null;
            }
        }

        private static string ReadLine(StreamReader file)
        {
            Debug.Assert(file != null);

            string line = RemoveUnusefulContent(file.ReadLine());
            while (String.IsNullOrEmpty(line) && file.Peek() > -1)
            {
                line = RemoveUnusefulContent(file.ReadLine());
            }

            return line;
        }

        private static string RemoveUnusefulContent(string line)
        {
            if (String.IsNullOrWhiteSpace(line))
            {
                return String.Empty;
            }

            // Remove the whitespace
            line = line.Trim();

            // Remove the comment
            int commentIdx = line.IndexOf('/');
            if (commentIdx > -1)
            {
                line = line.Substring(0, commentIdx);
            }

            return line;
        }
    }
}