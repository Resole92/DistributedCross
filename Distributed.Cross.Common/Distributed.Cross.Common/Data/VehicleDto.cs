using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Data
{
    public class VehicleDto
    {
        public int Priority { get; set; } = 1;
        public int InputLane { get; set; }
        public int OutputLane { get; set; }
        public double Speed { get; set; }
        public int? BrokenNode { get; set; }
        public int LicensePlate { get; set; }


        public VehicleDto Clone()
        => new VehicleDto
        {
            Priority = Priority,
            InputLane = InputLane,
            OutputLane = OutputLane,
            Speed = Speed,
            BrokenNode = BrokenNode,
            LicensePlate = LicensePlate,
        };

        public override string ToString()
        {
            return $"Vehicle {LicensePlate} start from lane {InputLane} to lane {OutputLane} with priority {Priority} and speed {Speed}";
        }

    }
}
