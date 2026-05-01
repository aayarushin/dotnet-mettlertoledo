using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omda.MettlerToledo.SICS
{
    internal abstract class Response
    {
        public const ushort ETXLength = 2;

        private readonly Request _request;

        protected virtual Request Request => _request;

        public static readonly byte[] ETX = new byte[] { (byte)'\r', (byte)'\n' };

#if NETSTANDARD
        protected Response(Request request, byte[] responseMessage)
#else
        protected Response(Request request, Memory<byte> responseMessage)
#endif
        {
            _request = request;

            if (responseMessage.Length < ETXLength)
            {
                throw new SICSException("The SICS Response Message Length was too short");
            }

#if NETSTANDARD
            if (responseMessage.Skip(responseMessage.Length - ETXLength).Take(ETXLength).SequenceEqual(ETX) == false)
            {
                throw new SICSException("Invalid or Missing ETX");
            }

            string messageString = Encoding.ASCII.GetString(responseMessage.Take(responseMessage.Length - ETXLength).ToArray());
#else


            if (responseMessage.Slice(responseMessage.Length - ETXLength, ETXLength).Span.SequenceEqual(ETX) == false)
            {
                throw new SICSException("Invalid or Missing ETX");
            }

            string messageString = Encoding.ASCII.GetString(responseMessage.Slice(0, responseMessage.Length - ETXLength).ToArray());
#endif

            if (messageString == "ES")
            {
                throw new SICSException("The SICS Command is not Supported by the Device");
            }

            if (messageString == "ET")
            {
                throw new SICSException("SICS Communication or Protocol Error");
            }

            if (messageString == "EL")
            {
                throw new SICSException("The SICS Command Parameters were Incorrect");
            }

            if (messageString == "EI")
            {
                throw new SICSException("The SICS Command cannot be Executed at this Time");
            }

            UnpackMessageDetail(request, messageString);
        }

        protected abstract void UnpackMessageDetail(Request request, string messageDetail);
    }
}
