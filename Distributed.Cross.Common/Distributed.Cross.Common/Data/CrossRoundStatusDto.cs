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
}
