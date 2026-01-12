using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace RICADO.MettlerToledo.SICS
{
    internal class ReadNetWeightResponse : Response
    {
        // Support both '.' and ',' as decimal separators for international compatibility
        private const string SuccessMessageRegex = @"^S [SD]\u0020+([0-9\.\,\-]+) (.*)$";
        private const string OutOfRangeMessageRegex = "^S [\u002B\\-\u002D]";
        private const string FailureMessageRegex = "^S [AI]";

        private double _netWeight;
        private string _units;

        public double NetWeight => _netWeight;
        public string Units => _units;

#if NETSTANDARD
        protected ReadNetWeightResponse(Request request, byte[] responseMessage) : base(request, responseMessage)
        {
        }
#else
        protected ReadNetWeightResponse(Request request, Memory<byte> responseMessage) : base(request, responseMessage)
        {
        }
#endif

#if NETSTANDARD
        public static ReadNetWeightResponse UnpackResponseMessage(ReadNetWeightRequest request, byte[] responseMessage)
        {
            return new ReadNetWeightResponse(request, responseMessage);
        }
#else
        public static ReadNetWeightResponse UnpackResponseMessage(ReadNetWeightRequest request, Memory<byte> responseMessage)
        {
            return new ReadNetWeightResponse(request, responseMessage);
        }
#endif

        protected override void UnpackMessageDetail(Request request, string messageDetail)
        {
            string[] regexSplit;

            if (Regex.IsMatch(messageDetail, SuccessMessageRegex))
            {
                regexSplit = Regex.Split(messageDetail, SuccessMessageRegex);
            }
            else if (Regex.IsMatch(messageDetail, OutOfRangeMessageRegex))
            {
                throw new SICSException("The Read Net Weight Response was Out of Range");
            }
            else if (Regex.IsMatch(messageDetail, FailureMessageRegex))
            {
                throw new SICSException("The Read Net Weight Request cannot be Executed at this Time");
            }
            else
            {
                throw new SICSException("The Read Net Weight Response Message Format was Invalid");
            }

            double weight;

            // Use invariant culture for parsing to handle dot decimal separator from SICS protocol
            if (double.TryParse(regexSplit[1], NumberStyles.Float, CultureInfo.InvariantCulture, out weight) == false)
            {
                throw new SICSException("Failed to Extract a Weight Value from the Read Net Weight Response");
            }

            _netWeight = weight;

            _units = regexSplit[2];
        }
    }
}
