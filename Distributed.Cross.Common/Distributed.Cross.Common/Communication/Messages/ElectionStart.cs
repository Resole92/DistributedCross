using Distributed.Cross.Common.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Communication.Messages
{
    public class ElectionStart
    {
        public List<int> LastRoundVehicleRunning { get; set; }
        public List<VehicleDto> Vehicles { get; set; }
    }
}
