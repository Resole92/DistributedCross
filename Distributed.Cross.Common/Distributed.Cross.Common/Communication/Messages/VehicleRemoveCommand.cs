using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Communication.Messages
{
    public class VehicleRemoveCommand
    {
    }

    public class VehicleExitNotification
    {
        public int StartLane { get; set; }
        public int Identifier { get; set; }
    }
}
