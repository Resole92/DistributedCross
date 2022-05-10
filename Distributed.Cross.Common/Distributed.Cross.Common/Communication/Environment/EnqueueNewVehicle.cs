using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Communication.Environment
{
    public class EnqueueNewVehicle
    {
        public int InputLane { get; set; }
        public int OutputLane { get; set; }

        /// <summary>
        /// Seconds for crossing
        /// </summary>
        public double Velocity { get; set; } = 3.5;
    }
}
