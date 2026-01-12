using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace RICADO.MettlerToledo.SICS
{
    internal class ReadTareWeightResponse : Response
    {
        // Support both '.' and ',' as decimal separators for international compatibility
        private const string SuccessMessageRegex = @"^TA A\u0020+([0-9\.\,\-]+) (.*)$";
        private const string FailureMessageRegex = "^TA [IL]";

        private double _tareWeight;
        private string _units;

        public double TareWeight => _tareWeight;
        public string Units => _units;

#if NETSTANDARD
        protected ReadTareWeightResponse(Request request, byte[] responseMessage) : base(request, responseMessage)
        {
        }
#else
        protected ReadTareWeightResponse(Request request, Memory<byte> responseMessage) : base(request, responseMessage)
        {
        }
#endif

#if NETSTANDARD
        public static ReadTareWeightResponse UnpackResponseMessage(ReadTareWeightRequest request, byte[] responseMessage)
        {
            return new ReadTareWeightResponse(request, responseMessage);
        }
#else
        public static ReadTareWeightResponse UnpackResponseMessage(ReadTareWeightRequest request, Memory<byte> responseMessage)
        {
            return new ReadTareWeightResponse(request, responseMessage);
        }
#endif

        protected override void UnpackMessageDetail(Request request, string messageDetail)
        {
            string[] regexSplit;

            if (Regex.IsMatch(messageDetail, SuccessMessageRegex))
            {
                regexSplit = Regex.Split(messageDetail, SuccessMessageRegex);
            }
            else if (Regex.IsMatch(messageDetail, FailureMessageRegex))
            {
                throw new SICSException("The Read Tare Weight Request cannot be Executed at this Time");
            }
            else
            {
                throw new SICSException("The Read Tare Weight Response Message Format was Invalid");
            }

            double weight;

            // Use invariant culture for parsing to handle dot decimal separator from SICS protocol
            if (double.TryParse(regexSplit[1], NumberStyles.Float, CultureInfo.InvariantCulture, out weight) == false)
            {
                throw new SICSException("Failed to Extract a Weight Value from the Read Tare Weight Response");
            }

            _tareWeight = weight;

            _units = regexSplit[2];
        }
    }
}
