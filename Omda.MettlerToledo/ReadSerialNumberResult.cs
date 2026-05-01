using System;

namespace Omda.MettlerToledo
{
    public class ReadSerialNumberResult : RequestResult
    {
        private readonly string _serialNumber;

        public string SerialNumber => _serialNumber;

        internal ReadSerialNumberResult(Channels.ProcessMessageResult result, string serialNumber) : base(result)
        {
            _serialNumber = serialNumber;
        }
    }
}
