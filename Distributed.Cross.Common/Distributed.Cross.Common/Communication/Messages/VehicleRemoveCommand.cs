using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Communication.Messages
{
    public class VehicleRemoveCommand
    {
        public int? BrokenNode { get; set; }
    }

    public class VehicleExitNotification
    {
        public int InputLane { get; set; }
        public int Identifier { get; set; }
        public int? BrokenNode { get; set; }
    }
}
