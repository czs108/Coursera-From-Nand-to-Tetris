using System.IO;
using System.Diagnostics;
using System;

namespace Project6
{
    public class CodeWriter : IDisposable
    {
        private StreamWriter outputFile = default;

        private readonly int bits;

        public CodeWriter(string filename, int bits)
        {
            Debug.Assert(!String.IsNullOrWhiteSpace(filename));

            this.bits = bits;
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

        public void WriteCommandA(int value)
        {
            outputFile.WriteLine($"0{Convert.ToString(value, 2).PadLeft(bits - 1, '0')}");
        }

        public void WriteCommandC(string comp, string dest, string jump)
        {
            outputFile.WriteLine($"111{Code.Comp(comp)}{Code.Dest(dest)}{Code.Jump(jump)}");
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