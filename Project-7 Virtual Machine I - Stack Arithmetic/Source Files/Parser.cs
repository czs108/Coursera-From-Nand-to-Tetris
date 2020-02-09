using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;

namespace Project7
{
    public class Parser : IDisposable
    {
        private static readonly HashSet<string> arithmeticCmds = new HashSet<string>()
        {
            "add", "sub", "neg", "eq", "gt", "lt", "and", "or", "not"
        };

        private StreamReader inputFile = null;

        private string currentLine = null;

        public Parser(string filename)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(filename));

            inputFile = new StreamReader(filename);
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

        public bool HasMoreCommands() => inputFile.Peek() > -1;

        public void Advance()
        {
            if (HasMoreCommands())
            {
                currentLine = RemoveWhitespace(inputFile.ReadLine());
            }
            else
            {
                currentLine = null;
            }
        }

        public CmdType CommandType()
        {
            if (String.IsNullOrWhiteSpace(currentLine))
            {
                return CmdType.Whitespace;
            }

            string[] fields = currentLine.Split(' ');
            string cmd = fields[0];
            if (arithmeticCmds.Contains(cmd))
            {
                return CmdType.Arithmetic;
            }

            switch (cmd)
            {
                case "push":
                {
                    return CmdType.Push;
                }
                case "pop":
                {
                    return CmdType.Pop;
                }
                case "label":
                {
                    return CmdType.Label;
                }
                case "goto":
                {
                    return CmdType.Goto;
                }
                case "if-goto":
                {
                    return CmdType.If;
                }
                case "function":
                {
                    return CmdType.Function;
                }
                case "call":
                {
                    return CmdType.Call;
                }
                case "return":
                {
                    return CmdType.Return;
                }
                default:
                {
                    throw new KeyNotFoundException(
                        " [!] The command is not in the set.");
                }
            }
        }

        public string Arg1()
        {
            Debug.Assert(CommandType() != CmdType.Return
                && CommandType() != CmdType.Whitespace);

            string[] fields = currentLine.Split(' ');
            if (CommandType() == CmdType.Arithmetic)
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
            Debug.Assert(CommandType() == CmdType.Push
                || CommandType() == CmdType.Pop
                || CommandType() == CmdType.Function
                || CommandType() == CmdType.Call);

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

        private static string RemoveWhitespace(string line)
        {
            if (String.IsNullOrWhiteSpace(line))
            {
                return null;
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