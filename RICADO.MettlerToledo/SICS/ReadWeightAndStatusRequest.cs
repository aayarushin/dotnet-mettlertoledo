using System;
using System.Text;

namespace RICADO.MettlerToledo.SICS
{
    internal class ReadWeightAndStatusRequest : Request
    {
        protected ReadWeightAndStatusRequest(string commandCode) : base(commandCode)
        {
        }

#if NETSTANDARD
        public ReadWeightAndStatusResponse UnpackResponseMessage(byte[] responseMessage)
        {
            return ReadWeightAndStatusResponse.UnpackResponseMessage(this, responseMessage);
        }
#else
        public ReadWeightAndStatusResponse UnpackResponseMessage(Memory<byte> responseMessage)
        {
            return ReadWeightAndStatusResponse.UnpackResponseMessage(this, responseMessage);
        }
#endif

        public static ReadWeightAndStatusRequest CreateNew(MettlerToledoDevice device)
        {
            return new ReadWeightAndStatusRequest(Commands.ReadWeightAndStatus);
        }

        protected override void BuildMessageDetail(ref StringBuilder messageBuilder)
        {
        }
    }
}
