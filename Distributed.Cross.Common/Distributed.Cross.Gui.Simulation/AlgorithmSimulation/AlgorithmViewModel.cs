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

            var crossMap = BuildMap();

            var trajectoryAlgorithm = new TrajectoryAlgorithm(crossMap);
            trajectoryAlgorithm.CreateGraphMatrixRappresentation();

            var trajectory1 = trajectoryAlgorithm.Calculate(1);
            var trajectory2 = trajectoryAlgorithm.Calculate(2);
            var trajectory3 = trajectoryAlgorithm.Calculate(3);
            var trajectory4 = trajectoryAlgorithm.Calculate(4);
        });

        public RelayCommand StartCollisionAlgorithmCommand
            => new RelayCommand(_ =>
            {
                var crossMap = BuildMap();


                var collisionAlgorithm = new CollisionAlgorithm(crossMap);

                var round = 0;
                do
                {
                    round++;
                    Console.WriteLine($"---------------------------");
                    Console.WriteLine($"Running round number {round}");
                    collisionAlgorithm.Calculate();
                }
                while (CheckCrossVehicle(collisionAlgorithm, crossMap, round) && round < 100);
                Console.WriteLine($"All vehicle cross!");


            });


        private List<int> VehiclesCross = new List<int>();

        private bool CheckCrossVehicle(CollisionAlgorithm collisionAlgorithm, CrossMap crossMap, int round)
        {
            foreach (var vehicleCross in VehiclesCross.ToList())
            {
                var isRunner = collisionAlgorithm.AmIRunner(vehicleCross);
                if (isRunner)
                {
                    Console.WriteLine($"Vehicle number {vehicleCross} is cross on round {round}...");
                    crossMap.RemoveVehicle(vehicleCross);
                    VehiclesCross.Remove(vehicleCross);
                }
            }

            collisionAlgorithm.IncrementPriority();

            if (round == 2)
            {
                crossMap.AddVehicle(new Common.Data.VehicleDto
                {
                    InputLane = 1,
                    OutputLane = 6,
                });
                VehiclesCross.Add(1);

                crossMap.AddVehicle(new Common.Data.VehicleDto
                {
                    InputLane = 2,
                    OutputLane = 5,
                });
                VehiclesCross.Add(2);
            }

            return VehiclesCross.Any();
        }

        public CrossMap BuildEmptyMap()
        {
            var builder = new CrossBuilder(3, 3);
            builder.CreateBasicInputOutput();
            var crossMap = builder.Build();
            return crossMap;
        }

        public CrossMap BuildMap()
        {

            var builder = new CrossBuilder(3, 3);
            builder.CreateBasicInputOutput();
            var crossMap = builder.Build();
            crossMap.AddBrokenNode(1);

            crossMap.AddVehicle(new Common.Data.VehicleDto
            {
                InputLane = 1,
                OutputLane = 6,
            });

            crossMap.AddVehicle(new Common.Data.VehicleDto
            {
                InputLane = 2,
                OutputLane = 5,
            });

            crossMap.AddVehicle(new Common.Data.VehicleDto
            {
                InputLane = 3,
                OutputLane = 7,
            });

            crossMap.AddVehicle(new Common.Data.VehicleDto
            {
                InputLane = 4,
                OutputLane = 8,
            });


            VehiclesCross = new List<int> { 1, 2, 3, 4 };

            return crossMap;
        }




    }
}
