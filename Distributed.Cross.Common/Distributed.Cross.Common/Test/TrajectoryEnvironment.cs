using Distributed.Cross.Common.Algorithm;
using Distributed.Cross.Common.Algorithm.Utilities;
using Distributed.Cross.Common.Data;
using Distributed.Cross.Common.Module;
using Distributed.Cross.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Test
{
    public class TrajectoryEnvironment
    {
        public bool Test(TrajectoryDataTest data)
        {
            var builder = data.Builder;
            var crossMap = builder.Build();

            data.BrokenNodes.ForEach(crossMap.AddBrokenNode);

            var vehicle = data.Vehicle;
            crossMap.AddVehicle(vehicle);

            var trajectoryAlgorithm = new TrajectoryAlgorithm(crossMap);
            trajectoryAlgorithm.CreateGraphMatrixRappresentation();
            var result = trajectoryAlgorithm.Calculate(vehicle.InputLane);

            var calculatedTrajectory = result.Trajectory;
            var expectedTrajectory = data.ExpectedTrajectory;

            if (expectedTrajectory.Count != calculatedTrajectory.Count) return data.MustBeTrue == false;

            for(var i = 0; i < expectedTrajectory.Count; i++)
            {
                if (expectedTrajectory[i] != calculatedTrajectory[i]) return data.MustBeTrue == false;
            }

            return data.MustBeTrue == true;

        }

    }

    public class TrajectoryDataTest
    {
        public CrossBuilder Builder { get; set; }

        public List<int> BrokenNodes { get; set; } = new List<int>();
        public VehicleDto Vehicle { get; set; }

        public List<int> ExpectedTrajectory { get; set; } = new List<int>();
        public bool MustBeTrue { get; set; }
    }
}
