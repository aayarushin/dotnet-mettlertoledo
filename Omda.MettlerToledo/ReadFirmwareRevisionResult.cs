using System;

namespace Omda.MettlerToledo
{
    public class ReadFirmwareRevisionResult : RequestResult
    {
        private readonly string _version;

        public string Version => _version;

        internal ReadFirmwareRevisionResult(Channels.ProcessMessageResult result, string version) : base(result)
        {
            _version = version;
        }
    }
}
