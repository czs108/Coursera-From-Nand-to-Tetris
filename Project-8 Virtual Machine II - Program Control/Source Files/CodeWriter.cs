using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System;

namespace Project8
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

        public void SetFileName(string filename)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(filename));

            code.SetFileNamePrefix(Path.GetFileNameWithoutExtension(filename));
        }

        public void WriteInit()
        {
            outputFile.WriteLine(code.Init());
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

        public void WriteGoto(string label)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(label));

            outputFile.WriteLine(code.Goto(label));
        }

        public void WriteLabel(string label)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(label));

            outputFile.WriteLine(code.Label(label));
        }

        public void WriteIf(string label)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(label));

            outputFile.WriteLine(code.If(label));
        }

        public void WriteFunction(string function, int vars)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(function));

            outputFile.WriteLine(code.Function(function, vars));
        }

        public void WriteCall(string function, int args)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(function));

            outputFile.WriteLine(code.Call(function, args));
        }

        public void WriteReturn() => outputFile.WriteLine(code.Return());

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