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
        public IReadOnlyList<int> BrokenNodes { get; set; }
        public NewLeaderNotification(int identifier, IReadOnlyList<VehicleDto> involvedVehicles, IReadOnlyList<int> brokenNodes)
        {
            Identifier = identifier;
            InvolvedVehicles = involvedVehicles;
            BrokenNodes = brokenNodes;
        }
    }

    public class NewLeaderNotificationResponse
    {
        public int RoundNumber { get; set; }
        public NewLeaderNotificationResponse(){}
    }
}
