using Distributed.Cross.Common.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Communication.Messages
{
    public class EnqueueNewVehicle
    {
        public VehicleDto Vehicle { get; set; }

        public EnqueueNewVehicle(VehicleDto vehicle)
        {
            Vehicle = vehicle.Clone();
        }
    }
}
