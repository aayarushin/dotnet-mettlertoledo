using System;

namespace RICADO.MettlerToledo
{
    public class CommandResult : RequestResult
    {
        internal CommandResult(Channels.ProcessMessageResult result) : base(result)
        {
        }
    }
}
