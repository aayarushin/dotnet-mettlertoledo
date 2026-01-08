using System;

namespace RICADO.MettlerToledo
{
    public class ReadTareWeightResult : RequestResult
    {
        private readonly double _tareWeight;
        private readonly string _units;

        public double TareWeight => _tareWeight;
        public string Units => _units;

        internal ReadTareWeightResult(Channels.ProcessMessageResult result, double tareWeight, string units) : base(result)
        {
            _tareWeight = tareWeight;
            _units = units;
        }
    }
}
