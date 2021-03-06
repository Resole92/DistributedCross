using Distributed.Cross.Common.Data;
using Distributed.Cross.Common.Test;
using Distributed.Cross.Common.Utilities;
using Distributed.Cross.Gui.Simulation.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Distributed.Cross.Gui.Simulation.AlgorithmSimulation
{
    /// <summary>
    /// Logica di interazione per AlgorithmUC.xaml
    /// </summary>
    public partial class AlgorithmUC : UserControl
    {
        public AlgorithmUC()
        {
            InitializeComponent();
        }
    }

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
                var allPositive = results.All(x => x);

                if (allPositive)
                {
                    MessageBox.Show("All test passed!","Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var resultsDisplayed = "Results are:" + string.Join(",", results);
                    MessageBox.Show($"Some test failed. {resultsDisplayed}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

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
            ExpectedTrajectory = new List<int> { 9, 12, 15 },
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
           ExpectedTrajectory = new List<int> { 11, 13, 15 },
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

                var tests = new List<CollisionDataTest> { CollisionTest1, CollisionTest2, CollisionTest3, CollisionTest4, CollisionTest5, CollisionTest6, CollisionTest7 };
                
                var results = tests.Select(collisionEnvironment.Test);
                var allPositive = results.All(x => x);

                if(allPositive)
                {
                    MessageBox.Show("All test passed!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var resultsDisplayed = "Results are:" + string.Join(",", results);
                    MessageBox.Show($"Some test failed. {resultsDisplayed}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning );
                }

            });

        #region Collision tests

        public CollisionDataTest CollisionTest1 =>
            new CollisionDataTest
            {
                Builder = BasicBuilder,
                Vehicles = new()
                {
                    new()
                    { InputLane = 1, OutputLane = 7 },
                    new()
                    { InputLane = 2, OutputLane = 8 },
                    new()
                    { InputLane = 3, OutputLane = 5 },
                    new()
                    { InputLane = 4, OutputLane = 6 },
                },
                Rounds = new()
                {
                    new()
                    { Number = 1, VehiclesNotRunning = new() { 2, 4 }, VehiclesRunning = new() { 1, 3 } },
                    new()
                    { Number = 2, VehiclesRunning = new() { 2, 4 } }

                },
                MustBeTrue = true,
            };

        public CollisionDataTest CollisionTest2 =>
            new CollisionDataTest
            {
                Builder = BasicBuilder,
                Vehicles = new()
                {
                    new()
                    { InputLane = 1, OutputLane = 6 },
                    new()
                    { InputLane = 2, OutputLane = 7 },
                    new()
                    { InputLane = 3, OutputLane = 8 },
                    new()
                    { InputLane = 4, OutputLane = 5 },
                },
                Rounds = new()
                {
                    new()
                    { Number = 1, VehiclesNotRunning = new() { 2, 3, 4 }, VehiclesRunning = new() { 1 } },
                    new()
                    { Number = 2, VehiclesNotRunning = new() { 3, 4 }, VehiclesRunning = new() { 2 } },
                    new()
                    { Number = 3, VehiclesNotRunning = new() { 4 }, VehiclesRunning = new() { 3 } },
                    new()
                    { Number = 4, VehiclesRunning = new() { 4 } }

                },
                MustBeTrue = true,
            };

        public CollisionDataTest CollisionTest3 =>
            new CollisionDataTest
            {
                Builder = BasicBuilder,
                Vehicles = new()
                {
                    new()
                    { InputLane = 1, OutputLane = 6 },
                    new()
                    { InputLane = 2, OutputLane = 7 },
                    new()
                    { InputLane = 3, OutputLane = 8 },
                    new()
                    { InputLane = 4, OutputLane = 5 },
                },
                Rounds = new()
                {
                    new()
                    { Number = 1, VehiclesNotRunning = new() { 2, 3, 4 }, VehiclesRunning = new() { 1 } },
                    new()
                    { Number = 2, VehiclesNotRunning = new() { 3, 4 }, VehiclesRunning = new() { 2 } },
                    new()
                    { Number = 3, VehiclesNotRunning = new() { 4 }, VehiclesRunning = new() { 3 } }

                },
                MustBeTrue = false,
            };


        public CollisionDataTest CollisionTest4 =>
            new CollisionDataTest
            {
                Builder = BasicBuilder,
                Vehicles = new()
                {
                    new()
                    { InputLane = 1, OutputLane = 8 },
                    new()
                    { InputLane = 1, OutputLane = 8 },
                    new()
                    { InputLane = 1, OutputLane = 8 },
                    new()
                    { InputLane = 2, OutputLane = 7 },
                    new()
                    { InputLane = 3, OutputLane = 5 },
                    new()
                    { InputLane = 4, OutputLane = 6 },
                },
                Rounds = new()
                {
                    new()
                    { Number = 1, VehiclesNotRunning = new() { 3, 4 }, VehiclesRunning = new() { 1, 2 } },
                    new()
                    { Number = 2, VehiclesNotRunning = new() { 4 }, VehiclesRunning = new() { 1, 3 } },
                    new()
                    { Number = 3, VehiclesRunning = new() { 1, 4 } }

                },
                MustBeTrue = true,
            };


        public CollisionDataTest CollisionTest5 =>
           new CollisionDataTest
           {
               Builder = BasicBuilder,
               Vehicles = new()
               {
                   new()
                   { InputLane = 1, OutputLane = 7 },
                   new()
                   { InputLane = 2, OutputLane = 8 },
                   new()
                   { InputLane = 3, OutputLane = 5 },
                   new()
                   { InputLane = 4, OutputLane = 6 },
               },
               BrokenNodes = new()
               {
                   17
               },
               Rounds = new()
               {
                   new()
                   { Number = 1, VehiclesNotRunning = new() { 2 }, VehiclesRunning = new() { 1 }, VehiclesBlocked = new() { 3, 4 } },
                   new()
                   { Number = 2, VehiclesRunning = new() { 2 } }
               },
               MustBeTrue = true,
           };

        public CollisionDataTest CollisionTest6 =>
           new CollisionDataTest
           {
               Builder = BasicBuilder,
               Vehicles = new()
               {
                   new()
                   { InputLane = 1, OutputLane = 7 },
                   new()
                   { InputLane = 2, OutputLane = 8 },
                   new()
                   { InputLane = 3, OutputLane = 5 },
                   new()
                   { InputLane = 4, OutputLane = 6 },
               },
               BrokenNodes = new()
               {
                   16
               },
               Rounds = new()
               {
                   new()
                   { Number = 1, VehiclesNotRunning = new() { 2, 4 }, VehiclesRunning = new() { 1, 3 } },
                   new()
                   { Number = 2, VehiclesRunning = new() { 2, 4 } }
               },
               MustBeTrue = true,
           };

        public CollisionDataTest CollisionTest7 =>
            new CollisionDataTest
            {
                Builder = BasicBuilder,
                Vehicles = new()
                {
                    new()
                    { InputLane = 1, OutputLane = 7 },
                    new()
                    { InputLane = 2, OutputLane = 8 },
                    new()
                    { InputLane = 3, OutputLane = 5 },
                    new()
                    { InputLane = 4, OutputLane = 6 },
                },
                BrokenNodes = new()
                {
                    6
                },
                Rounds = new()
                {
                    new()
                    { Number = 1, VehiclesNotRunning = new() { 2 }, VehiclesRunning = new() { 1, 3 }, VehiclesBlocked = new() { 4 } },
                    new()
                    { Number = 2, VehiclesRunning = new() { 2 } }
                },
                MustBeTrue = true,
            };

        #endregion 

        public CrossBuilder BasicBuilder => new CrossBuilder(3, 3).CreateBasicInputOutput();



    }

}
