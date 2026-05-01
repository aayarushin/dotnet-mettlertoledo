using System;

namespace Omda.MettlerToledo
{
    public enum CommandType
    {
        ZeroStable,
        ZeroImmediately,
        TareStable,
        TareImmediately,
        ClearTare,
    }

    public enum WeightType
    {
        Gross,
        Tare,
        Net
    }

    public enum ConnectionMethod
    {
        Ethernet,
        Serial
    }

    public enum ProtocolType
    {
        SICS
    }
}
