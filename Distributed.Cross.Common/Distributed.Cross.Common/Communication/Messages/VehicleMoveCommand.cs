using Distributed.Cross.Common.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Communication.Messages
{
    public class VehicleMoveCommand
    {
        public VehicleDto Vehicle{ get; set; }
        public List<VehicleDto> AllVehicles { get; set; }
        public int LeaderIdentifier { get; set; }
        public List<int> VehiclesRunning { get; set; }

    }

    public class VehicleMoveNotification
    {
        public int InputLane { get; set; }
        public int OutputLane { get; set; }
        public double Velocity { get; set; }
    }
}
