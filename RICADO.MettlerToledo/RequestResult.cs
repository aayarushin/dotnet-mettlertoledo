using System;

namespace RICADO.MettlerToledo
{
    public abstract class RequestResult
    {
        public int _bytesSent = 0;
        public int _packetsSent = 0;
        public int _bytesReceived = 0;
        public int _packetsReceived = 0;
        public double _duration = 0;

        public int BytesSent => _bytesSent;
        public int PacketsSent => _packetsReceived;
        public int BytesReceived => _bytesReceived;
        public int PacketsReceived => _packetsReceived;
        public double Duration => _duration;

        internal RequestResult()
        {
        }

        internal RequestResult(Channels.ProcessMessageResult result)
        {
            _bytesSent = result.BytesSent;
            _packetsSent = result.PacketsSent;
            _bytesReceived = result.BytesReceived;
            _packetsReceived = result.PacketsReceived;
            _duration = result.Duration;
        }

        internal void AddMessageResult(Channels.ProcessMessageResult result)
        {
            _bytesSent += result.BytesSent;
            _packetsSent += result.PacketsSent;
            _bytesReceived += result.BytesReceived;
            _packetsReceived += result.PacketsReceived;
            _duration += result.Duration;
        }
    }
}
