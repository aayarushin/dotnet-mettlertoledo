using System;

namespace Omda.MettlerToledo.Channels
{
    internal struct ReceiveMessageResult
    {
#if NETSTANDARD
        internal byte[] Message;
#else
        internal Memory<byte> Message;
#endif
        internal int Bytes;
        internal int Packets;
    }
}
