using Distributed.Cross.Common.Algorithm;
using Distributed.Cross.Common.Module;
using Distributed.Cross.Common.Utilities;
using Distributed.Cross.Gui.Simulation.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distributed.Cross.Gui.Simulation.AlgorithmSimulation
{
    public class AlgorithmViewModel
    {
        private static AlgorithmViewModel _instance;
        public static AlgorithmViewModel Instance => _instance ??= new AlgorithmViewModel();

        AlgorithmViewModel()
        {

        }

        public RelayCommand StartTrajectoryAlgorithmCommand
        => new RelayCommand(_ =>
        {
            var builder = new CrossBuilder(3,3);
            builder.CreateBasicInputOutput();
            var crossMap = builder.Build();

            var trajectoryAlgorithm = new TrajectoryAlgorithm(crossMap);
            trajectoryAlgorithm.CreateGraphMatrixRappresentation();
            var vehicle1 = new Vehicle
            {
                DestinationLane = 6,
            };
            var vehicle2 = new Vehicle
            {
                DestinationLane = 5,
            };
            var vehicle3 = new Vehicle
            {
                DestinationLane = 7,
            };
            var vehicle4 = new Vehicle
            {
                DestinationLane = 8,
            };

            crossMap.AddVehicle(vehicle1, 1);
            crossMap.AddVehicle(vehicle2, 2);
            crossMap.AddVehicle(vehicle3, 3);
            crossMap.AddVehicle(vehicle4, 4);

            var trajectory1 = trajectoryAlgorithm.Calculate(1);
            var trajectory2 = trajectoryAlgorithm.Calculate(2);
            var trajectory3 = trajectoryAlgorithm.Calculate(3);
            var trajectory4 = trajectoryAlgorithm.Calculate(4);
        });

        public RelayCommand StartCollisionAlgorithmCommand
            => new RelayCommand(_ =>
            {

            })



     
    }
}
