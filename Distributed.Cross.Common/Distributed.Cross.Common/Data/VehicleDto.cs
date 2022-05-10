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
        public double Velocity { get; set; }


        public VehicleDto Clone()
        => new VehicleDto
        {
            Priority = Priority,
            InputLane = InputLane,
            OutputLane = OutputLane,
            Velocity = Velocity,
        };

        public override string ToString()
        {
            return $"Vehicle start from lane {InputLane} to lane {OutputLane} with priority {Priority}";
        }

    }
}
