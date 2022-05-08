using Distributed.Cross.Common.Data;
using Distributed.Cross.Common.Module;
using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Communication.Messages
{
    public class VehicleOnNodeCommand
    {
        public VehicleDto Vehicle { get; set; }
      
    }

    public class VehicleOnNodeNotification
    {
        public int Identifier { get; set; }
        public bool IsAdded { get; set; }
    }
}
