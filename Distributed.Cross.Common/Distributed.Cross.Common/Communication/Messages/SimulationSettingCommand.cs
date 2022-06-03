using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Communication.Messages
{
    public class SimulationSettingCommand
    {
        /// <summary>
        /// After this number of vehicle spawn a random broken vehicle is inserted
        /// </summary>
        public int NumberVehicleForBroke { get; set; }
        /// <summary>
        /// Time in seconds 
        /// </summary>
        public int TimeForRepair { get; set; }
    }
}
