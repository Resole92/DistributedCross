using Distributed.Cross.Common.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Communication.Messages
{
    public class VehicleMoveNotification
    {
        public VehicleDto Vehicle{ get; set; }
        public List<VehicleDto> AllVehicles { get; set; }
        public int LeaderIdentifier { get; set; }
        public List<int> VehiclesRunning { get; set; }

    }
}
