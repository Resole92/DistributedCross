using Distributed.Cross.Common.Algorithm;
using Distributed.Cross.Common.Data;
using Distributed.Cross.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Distributed.Cross.Common.Test
{
    public class CrossRoundEnvironment
    {
        public bool IsWrongLeader { get; private set; }
        public bool ActualLeader { get; private set; }
        public bool ExpectedLeader { get; private set; }


        public bool IsRunnerButMustNotRun { get; private set; }
        public List<int> VehiclesRunnerButNot { get; private set; } = new();

        public bool IsNotRunnerButMustRun { get; private set; }
        public List<int> VehiclesNotRunnerButMust { get; private set; } = new();


        private CrossBuilder _builder;

        public CrossRoundEnvironment(CrossBuilder builder)
        {
            _builder = builder;
        }

        public bool Test(CrossRoundStatusDto crossData)
        {

            IsWrongLeader = false;
            IsRunnerButMustNotRun = false;
            IsNotRunnerButMustRun = false;

            var expectedLeader = crossData.Vehicles.Max(x => x.InputLane);
            var leader = crossData.LeaderVehicle;

            if (expectedLeader != leader)
            {
                IsWrongLeader = true;
            }


            var crossMap = _builder.Build();
            crossData.BrokenNode.ForEach(crossMap.AddBrokenNode);
            crossData.Vehicles.ForEach(crossMap.AddVehicle);


            var algorithm = new CollisionAlgorithm(crossMap);
            algorithm.Calculate();
            var vehiclesIdentifier = crossData.Vehicles.Select(x => x.InputLane);

            foreach (var vehicleIdentifier in vehiclesIdentifier)
            {
                if (algorithm.AmIRunner(vehicleIdentifier))
                {
                    var vehicleFound = crossData.VehiclesRunning.FirstOrDefault(x => x == vehicleIdentifier);
                    if (vehicleFound is 0)
                    {
                        IsNotRunnerButMustRun = true;
                        VehiclesNotRunnerButMust.Add(vehicleIdentifier);

                    }
                }
                else
                {
                    var vehicleFound = crossData.VehiclesRunning.FirstOrDefault(x => x == vehicleIdentifier);
                    if (vehicleFound is not 0)
                    {
                        IsRunnerButMustNotRun = true;
                        VehiclesRunnerButNot.Add(vehicleIdentifier);
                    }
                }
            }

            return !IsWrongLeader && !IsNotRunnerButMustRun && !IsRunnerButMustNotRun;
        }
    }
}
