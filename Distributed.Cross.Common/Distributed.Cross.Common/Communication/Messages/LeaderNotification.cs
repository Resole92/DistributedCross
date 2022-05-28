using Distributed.Cross.Common.Data;
using Distributed.Cross.Common.Module;
using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Communication.Messages
{
    public class LeaderNotificationRequest
    {
        public int Identifier { get; set; }
    }

    public class LeaderNotificationResponse
    {
        public VehicleDto VehicleDetail { get; set; }
        public bool Acknowledge { get; set; } = true;
    }

    public class NewLeaderNotification
    {
        public int Identifier { get; set; }
        public IReadOnlyList<VehicleDto> InvolvedVehicles { get; set; }
        public NewLeaderNotification(int identifier, IReadOnlyList<VehicleDto> involvedVehicles)
        {
            Identifier = identifier;
            InvolvedVehicles = involvedVehicles;
        }
    }
}
