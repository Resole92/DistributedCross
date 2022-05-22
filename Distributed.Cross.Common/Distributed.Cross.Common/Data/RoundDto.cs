using Distributed.Cross.Common.Algorithm.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Distributed.Cross.Common.Data
{
    public class RoundDto
    {
        public int Number { get; set; }
        public List<int> VehiclesRunning { get; set; } = new();
        public List<int> VehiclesNotRunning { get; set; } = new();

        public bool IsSameRound(RoundDto round)
        {

            if (VehiclesRunning.Except(round.VehiclesRunning).Any()) return false;
            if (round.VehiclesRunning.Except(VehiclesRunning).Any()) return false;

            if (VehiclesNotRunning.Except(round.VehiclesNotRunning).Any()) return false;
            if (round.VehiclesNotRunning.Except(VehiclesNotRunning).Any()) return false;

            return true;

        }


    }
}
