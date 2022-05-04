using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Data
{
    public class VehicleDto
    {
        public int Priority { get; set; } = 1;
        public int StartLane { get; set; }
        public int DestinationLane { get; set; }


        public VehicleDto Clone()
        => new VehicleDto
        {
            Priority = Priority,
            StartLane = StartLane,
            DestinationLane = DestinationLane
        };

        public override string ToString()
        {
            return $"Vehicle start from lane {StartLane} to lane {DestinationLane} with priority {Priority}";
        }

    }
}
