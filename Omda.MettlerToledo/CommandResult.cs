using System;

namespace Omda.MettlerToledo
{
    public class CommandResult : RequestResult
    {
        internal CommandResult(Channels.ProcessMessageResult result) : base(result)
        {
        }
    }
}
