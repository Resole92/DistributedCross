using Distributed.Cross.Common.Algorithm;
using Distributed.Cross.Common.Algorithm.Utilities;
using Distributed.Cross.Common.Data;
using Distributed.Cross.Common.Module;
using Distributed.Cross.Common.Test;
using Distributed.Cross.Common.Utilities;
using Distributed.Cross.Gui.Simulation.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Distributed.Cross.Gui.Simulation.AlgorithmSimulation
{
    public class AlgorithmViewModel
    {
        private static AlgorithmViewModel _instance;
        public static AlgorithmViewModel Instance => _instance ??= new AlgorithmViewModel();

        AlgorithmViewModel()
        {

        }

        public RelayCommand TrajectoryTestCommand
            => new RelayCommand(_ =>
            {
                var trajectoryEnvironment = new TrajectoryEnvironment();

                var tests = new List<TrajectoryDataTest> { TrajectoryTest1, TrajectoryTest2, TrajectoryTest3, TrajectoryTest4, TrajectoryTest5 };
                var results = tests.Select(trajectoryEnvironment.Test);
                var resultsDisplayed = "Results are:" + string.Join(",", results);
                MessageBox.Show(resultsDisplayed);
            });


        #region Trajectory tests

        public TrajectoryDataTest TrajectoryTest1
        => new TrajectoryDataTest
        {
            Builder = BasicBuilder,
            Vehicle = new VehicleDto
            {
                InputLane = 1,
                OutputLane = 7,
            },
            ExpectedTrajectory = new List<int> { 9,12,15},
            MustBeTrue = true
        };

        public TrajectoryDataTest TrajectoryTest2
        => new TrajectoryDataTest
        {
            Builder = BasicBuilder,
            Vehicle = new VehicleDto
            {
                InputLane = 1,
                OutputLane = 7,
            },
            ExpectedTrajectory = new List<int> { 9, 17, 15 },
            MustBeTrue = false
        };

        public TrajectoryDataTest TrajectoryTest3
        => new TrajectoryDataTest
        {
            Builder = BasicBuilder,
            Vehicle = new VehicleDto
            {
                InputLane = 1,
                OutputLane = 7,
            },
            ExpectedTrajectory = new List<int> { 9, 13, 15 },
            BrokenNodes = new List<int> { 12 },
            MustBeTrue = true,
        };

        public TrajectoryDataTest TrajectoryTest4
       => new TrajectoryDataTest
       {
           Builder = BasicBuilder,
           Vehicle = new VehicleDto
           {
               InputLane = 2,
               OutputLane = 7,
           },
           ExpectedTrajectory = new List<int> { 11,13,15 },
           MustBeTrue = true,
       };

        public TrajectoryDataTest TrajectoryTest5
       => new TrajectoryDataTest
       {
           Builder = BasicBuilder,
           Vehicle = new VehicleDto
           {
               InputLane = 1,
               OutputLane = 8,
           },
           ExpectedTrajectory = new List<int> { 9 },
           MustBeTrue = true,
       };

        #endregion


        public RelayCommand CollisionTestCommand
            => new RelayCommand(_ =>
            {
                var collisionEnvironment = new CollisionEnvironment();
                var result = collisionEnvironment.Test(CollisionTest1);
                MessageBox.Show($"Result is {result}");



            });

        #region Collision tests

        public CollisionDataTest CollisionTest1 =>
            new CollisionDataTest
            {
                Builder = BasicBuilder,
                Vehicles = new ()
                {
                    new () {InputLane = 1, OutputLane = 7},
                    new () {InputLane = 2, OutputLane = 8},
                    new () {InputLane = 3, OutputLane = 5},
                    new () {InputLane = 4, OutputLane = 6},
                },
                Rounds = new ()
                {
                    new () {Number = 1, VehiclesNotRunning = new () {2, 4}, VehiclesRunning  = new() {1, 3} },
                    new () {Number = 2, VehiclesRunning = new() {2, 4} }

                }
            };
        #endregion 

        public CrossBuilder BasicBuilder => new CrossBuilder(3, 3).CreateBasicInputOutput();

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

            var roundDto = new RoundDto
            {
                Number = 1,
                //Trajectories = new List<TrajectoryResult> { trajectory1, trajectory2, trajectory3, trajectory4 }
            };


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
