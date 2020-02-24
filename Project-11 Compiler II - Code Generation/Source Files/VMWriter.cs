using System.IO;
using System.Diagnostics;
using System;

namespace Project11
{
    public class VMWriter : IDisposable
    {
        private StreamWriter outputFile = null;

        public VMWriter(string filename)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(filename));

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
        ~VMWriter()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }
        public void WriteArithmetic(string command)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(command));

            outputFile.WriteLine(command);
        }

        public void WritePush(string segment, int index)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(segment));

            outputFile.WriteLine($"push {segment} {index}");
        }

        public void WritePop(string segment, int index)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(segment));
            Debug.Assert(segment != "constant");

            outputFile.WriteLine($"pop {segment} {index}");
        }

        public void WriteGoto(string label)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(label));

            outputFile.WriteLine($"goto {label}");
        }

        public void WriteLabel(string label)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(label));

            outputFile.WriteLine($"label {label}");
        }

        public void WriteIf(string label)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(label));

            outputFile.WriteLine($"if-goto {label}");
        }

        public void WriteFunction(string function, int vars)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(function));

            outputFile.WriteLine($"function {function} {vars}");
        }

        public void WriteCall(string function, int args)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(function));

            outputFile.WriteLine($"call {function} {args}");
        }

        public void WriteReturn() => outputFile.WriteLine("return");

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