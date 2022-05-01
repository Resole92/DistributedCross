using Distributed.Cross.Common.Module;
using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Communication.Messages
{
    public class CoordinationNotificationRequest
    {
        public List<Vehicle> VehiclesDetail { get; set; }
    }

    public class CoordinationNotificationResponse
    {

    }
}
