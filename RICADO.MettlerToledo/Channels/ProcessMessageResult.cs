using System;

namespace RICADO.MettlerToledo.Channels
{
    public struct ProcessMessageResult
    {
        public int BytesSent;
        public int PacketsSent;
        public int BytesReceived;
        public int PacketsReceived;
        public double Duration;

#if NETSTANDARD
        public byte[] ResponseMessage;
#else
        public Memory<byte> ResponseMessage;
#endif
    }
}
