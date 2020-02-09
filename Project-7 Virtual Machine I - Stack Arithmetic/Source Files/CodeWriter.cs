using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System;

namespace Project7
{
    public class CodeWriter : IDisposable
    {
        private StreamWriter outputFile = null;

        private Code code = null;

        public CodeWriter(string filename)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(filename));

            code = new Code(Path.GetFileNameWithoutExtension(filename));
            outputFile = new StreamWriter(filename);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // NOTE: Leave out the finalizer altogether if this class doesn't
        // own unmanaged resources itself, but leave the other methods
        // exactly as they are.
        ~CodeWriter()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        public void WriteArithmetic(string command)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(command));

            outputFile.WriteLine($"// {command}");
            outputFile.WriteLine(code.Arithmetic(command));
        }

        public void WritePushPop(string command, string segment, int index)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(command));
            Debug.Assert(!String.IsNullOrWhiteSpace(segment));

            outputFile.WriteLine($"// {command} {segment} {index}");

            if (command == "push")
            {
                outputFile.WriteLine(code.Push(segment, index));
            }
            else if (command == "pop")
            {
                outputFile.WriteLine(code.Pop(segment, index));
            }
            else
            {
                throw new KeyNotFoundException(
                    " [!] The command is not in the set.");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free managed resources
                outputFile?.Close();
                outputFile = null;
            }
        }
    }
}