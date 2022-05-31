using Distributed.Cross.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Distributed.Cross.Common.Communication.Messages
{
    public class VehicleMoveCommand
    {
        public List<int> CrossNode { get; set; }
        public VehicleDto Vehicle { get; set; }
        public List<VehicleDto> AllVehicles { get; set; }
        public int LeaderIdentifier { get; set; }
        public List<int> VehiclesRunning { get; set; }

        //Only for simulation
        public int ActualRound { get; set; }

    }

    public class VehicleMoveNotification
    {
        public List<int> CrossingNodes { get; set; }
        public VehicleDto Vehicle { get; set; }
        public VehicleMoveNotification(VehicleDto vehicle, IEnumerable<int> crossingNodes)
        {
            Vehicle = vehicle.Clone();
            CrossingNodes = crossingNodes.ToList();
        }

    }
}
