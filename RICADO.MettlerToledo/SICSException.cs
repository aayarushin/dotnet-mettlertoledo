using System;

namespace RICADO.MettlerToledo
{
    public class SICSException : Exception
    {
        internal SICSException(string message) : base(message)
        {
        }

        internal SICSException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
