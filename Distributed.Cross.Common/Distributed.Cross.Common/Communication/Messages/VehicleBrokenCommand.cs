using Distributed.Cross.Common.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Communication.Messages
{

    public class VehicleBrokenCommand
    {
        public VehicleDto Vehicle { get; set; }

        public VehicleBrokenCommand(VehicleDto vehicle)
        {
            Vehicle = vehicle;
        }
    }

    public class VehicleBrokenNotification
    {
        public VehicleDto Vehicle { get; set; }
        public VehicleBrokenNotification(VehicleDto vehicle)
        {
            Vehicle = vehicle;
        }
    }
}
