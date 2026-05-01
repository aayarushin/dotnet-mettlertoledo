using System;
using System.Text;

namespace Omda.MettlerToledo.SICS
{
    internal abstract class Request
    {
        public const string ETX = "\r\n";

        private readonly string _commandCode;

        public string CommandCode => _commandCode;

        protected Request(string commandCode)
        {
            _commandCode = commandCode;
        }

#if NETSTANDARD
        public byte[] BuildMessage()
#else
        public ReadOnlyMemory<byte> BuildMessage()
#endif
        {
            StringBuilder messageBuilder = new StringBuilder();

            messageBuilder.Append(_commandCode);

            BuildMessageDetail(ref messageBuilder);

            messageBuilder.Append(ETX);

            return Encoding.ASCII.GetBytes(messageBuilder.ToString());
        }

        protected abstract void BuildMessageDetail(ref StringBuilder messageBuilder);
    }
}
