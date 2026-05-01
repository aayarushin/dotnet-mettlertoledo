using System;

namespace Omda.MettlerToledo
{
    public class ReadNetWeightResult : RequestResult
    {
        private readonly double _netWeight;
        private readonly string _units;

        public double NetWeight => _netWeight;
        public string Units => _units;

        internal ReadNetWeightResult(Channels.ProcessMessageResult result, double netWeight, string units) : base(result)
        {
            _netWeight = netWeight;
            _units = units;
        }
    }
}
