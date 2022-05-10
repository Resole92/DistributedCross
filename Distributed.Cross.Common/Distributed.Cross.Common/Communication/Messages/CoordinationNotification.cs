using Distributed.Cross.Common.Data;
using Distributed.Cross.Common.Module;
using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Communication.Messages
{
    public class CoordinationNotification
    {
        public List<VehicleDto> VehiclesDetail { get; set; }
        public List<int> BrokenNodes { get; set; }
    }

}
