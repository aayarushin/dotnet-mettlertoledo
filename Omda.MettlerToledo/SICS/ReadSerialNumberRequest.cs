using System;
using System.Text;

namespace Omda.MettlerToledo.SICS
{
    internal class ReadSerialNumberRequest : Request
    {
        protected ReadSerialNumberRequest(string commandCode) : base(commandCode)
        {
        }

#if NETSTANDARD
        public ReadSerialNumberResponse UnpackResponseMessage(byte[] responseMessage)
        {
            return ReadSerialNumberResponse.UnpackResponseMessage(this, responseMessage);
        }
#else
        public ReadSerialNumberResponse UnpackResponseMessage(Memory<byte> responseMessage)
        {
            return ReadSerialNumberResponse.UnpackResponseMessage(this, responseMessage);
        }
#endif

        public static ReadSerialNumberRequest CreateNew(MettlerToledoDevice device)
        {
            return new ReadSerialNumberRequest(Commands.ReadSerialNumber);
        }

        protected override void BuildMessageDetail(ref StringBuilder messageBuilder)
        {
        }
    }
}
