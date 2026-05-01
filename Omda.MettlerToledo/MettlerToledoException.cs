using System;

namespace Omda.MettlerToledo
{
    public class MettlerToledoException : Exception
    {
        internal MettlerToledoException(string message) : base(message)
        {
        }

        internal MettlerToledoException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
