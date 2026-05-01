using System;
using System.Text;

namespace Omda.MettlerToledo.SICS
{
    internal class ReadFirmwareRevisionRequest : Request
    {
        protected ReadFirmwareRevisionRequest(string commandCode) : base(commandCode)
        {
        }

#if NETSTANDARD
        public ReadFirmwareRevisionResponse UnpackResponseMessage(byte[] responseMessage)
        {
            return ReadFirmwareRevisionResponse.UnpackResponseMessage(this, responseMessage);
        }
#else
        public ReadFirmwareRevisionResponse UnpackResponseMessage(Memory<byte> responseMessage)
        {
            return ReadFirmwareRevisionResponse.UnpackResponseMessage(this, responseMessage);
        }
#endif

        public static ReadFirmwareRevisionRequest CreateNew(MettlerToledoDevice device)
        {
            return new ReadFirmwareRevisionRequest(Commands.ReadFirmwareRevision);
        }

        protected override void BuildMessageDetail(ref StringBuilder messageBuilder)
        {
        }
    }
}
