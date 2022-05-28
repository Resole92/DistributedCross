using Distributed.Cross.Common.Algorithm.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Distributed.Cross.Common.Data
{
    public class CrossRoundStatusDto
    {
        public int Number { get; set; }
        public List<VehicleDto> Vehicles { get; set; } = new();
        public List<int> BrokenNode { get; set; } = new();
        public List<int> VehiclesRunning { get; set; } = new();
        public List<int> VehiclesNotRunning => Vehicles.Select(x => x.InputLane).Except(VehiclesRunning).ToList();
        public int LeaderVehicle { get; set; }



    }

    public class RoundDto
    {
        public int Number { get; set; }
        public List<int> VehiclesRunning { get; set; } = new();
        public List<int> VehiclesNotRunning { get; set; } = new();
        public List<int> VehiclesBlocked { get; set; } = new();
        public int LeaderVehicle { get; set; }


        public bool IsSameRound(RoundDto round)
        {

            if (VehiclesRunning.Except(round.VehiclesRunning).Any()) return false;
            if (round.VehiclesRunning.Except(VehiclesRunning).Any()) return false;

            if (VehiclesNotRunning.Except(round.VehiclesNotRunning).Any()) return false;
            if (round.VehiclesNotRunning.Except(VehiclesNotRunning).Any()) return false;

            if (VehiclesBlocked.Except(round.VehiclesBlocked).Any()) return false;
            if (round.VehiclesBlocked.Except(VehiclesBlocked).Any()) return false;

            return true;

        }


    }
}
