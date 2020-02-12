using System.Diagnostics;
using System.IO;
using System;

namespace Project6
{
    public class Parser : IDisposable
    {
        private StreamReader inputFile = default;

        private string currentLine = default;

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
                currentLine = default;
            }
        }

        public CmdType CommandType()
        {
            if (String.IsNullOrWhiteSpace(currentLine))
            {
                return CmdType.Whitespace;
            }

            if (currentLine.IndexOf('@') > -1)
            {
                return CmdType.A;
            }

            if (currentLine.IndexOf('(') > -1
                && currentLine.IndexOf(')') > -1)
            {
                return CmdType.Label;
            }

            return CmdType.C;
        }

        public string Symbol()
        {
            Debug.Assert(CommandType() == CmdType.A
                || CommandType() == CmdType.Label);

            if (CommandType() == CmdType.A)
            {
                return currentLine.Substring(currentLine.IndexOf('@') + 1);
            }
            else
            {
                int left = currentLine.IndexOf('(');
                int right = currentLine.IndexOf(')');
                return currentLine.Substring(left + 1, right - left - 1);
            }
        }

        public string Dest()
        {
            Debug.Assert(CommandType() == CmdType.C);

            int right = currentLine.IndexOf('=');
            if (right > -1)
            {
                return currentLine.Substring(0, right);
            }
            else
            {
                return default;
            }
        }

        public string Comp()
        {
            Debug.Assert(CommandType() == CmdType.C);

            int left = currentLine.IndexOf('=');
            if (left <= -1)
            {
                left = -1;
            }

            int right = currentLine.IndexOf(';');
            if (right <= -1)
            {
                right = currentLine.Length;
            }

            return currentLine.Substring(left + 1, right - left - 1);
        }

        public string Jump()
        {
            Debug.Assert(CommandType() == CmdType.C);

            int left = currentLine.IndexOf(';');
            if (left > -1)
            {
                return currentLine.Substring(left + 1);
            }
            else
            {
                return default;
            }
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
                return default;
            }

            // Remove the whitespace
            line = line.Replace(" ", String.Empty);

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