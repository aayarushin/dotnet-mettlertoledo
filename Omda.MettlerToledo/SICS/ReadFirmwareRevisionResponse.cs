using System;
using System.Text.RegularExpressions;

namespace Omda.MettlerToledo.SICS
{
    internal class ReadFirmwareRevisionResponse : Response
    {
        private const string MessageRegex = "^I3 A \"([0-9A-Za-z\u002E]+)\"$";

        private string _version;

        public string Version => _version;

#if NETSTANDARD
        protected ReadFirmwareRevisionResponse(Request request, byte[] responseMessage) : base(request, responseMessage)
        {
        }
#else
        protected ReadFirmwareRevisionResponse(Request request, Memory<byte> responseMessage) : base(request, responseMessage)
        {
        }
#endif

#if NETSTANDARD
        public static ReadFirmwareRevisionResponse UnpackResponseMessage(ReadFirmwareRevisionRequest request, byte[] responseMessage)
        {
            return new ReadFirmwareRevisionResponse(request, responseMessage);
        }
#else
        public static ReadFirmwareRevisionResponse UnpackResponseMessage(ReadFirmwareRevisionRequest request, Memory<byte> responseMessage)
        {
            return new ReadFirmwareRevisionResponse(request, responseMessage);
        }
#endif

        protected override void UnpackMessageDetail(Request request, string messageDetail)
        {
            string[] regexSplit;

            if (Regex.IsMatch(messageDetail, MessageRegex))
            {
                regexSplit = Regex.Split(messageDetail, MessageRegex);
            }
            else
            {
                throw new SICSException("The Read Firmware Revision Response Message Format was Invalid");
            }

            _version = regexSplit[1];
        }
    }
}
