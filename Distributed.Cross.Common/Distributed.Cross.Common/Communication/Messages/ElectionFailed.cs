using Distributed.Cross.Common.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Communication.Messages
{


    public class ElectionResult
    {
        public ElectionResultType Result { get; set; }
        public IReadOnlyList<VehicleDto> InvolvedVehicles { get; set; }
        public IReadOnlyList<int> BrokenNodes { get; set; }
        public ElectionResult(ElectionResultType result, IReadOnlyList<VehicleDto> involvedVehicles = null, IReadOnlyList<int> brokenNodes = null)
        {
            Result = result;
            InvolvedVehicles = involvedVehicles ?? new List<VehicleDto>();
            BrokenNodes = brokenNodes ?? new List<int>();
        }
    }

    public enum ElectionResultType
    {
        
        Cancelled,
        Crossing,
        Bully,
        LeaderAlreadyPresent,
        Elected,
        NotHandled
    }
}
