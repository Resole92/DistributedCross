using Akka.Actor;
using Distributed.Cross.Common.Algorithm;
using Distributed.Cross.Common.Communication.Messages;
using Distributed.Cross.Common.Data;
using Distributed.Cross.Common.Module;
using Distributed.Cross.Common.Utilities;
using Distributed.Cross.Gui.Simulation.AlgorithmSimulation;
using Distributed.Cross.Gui.Simulation.Environment;
using Distributed.Cross.Gui.Simulation.Environment.Components;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Distributed.Cross.Common.Actors
{
    public class EnvironmentActor : ReceiveActor
    {
        private int _licensePlateCount = 1;
        private int _numberVehicleForBroke = 5;
        private int _numberOfVehiclesSpawned = 0;
        private int _secondForRemoving = 15;



        private List<CrossRoundStatusDto> _rounds { get; set; } = new();
        private CrossRoundStatusDto _actualRound; 

        public Dictionary<int, IActorRef> ActorsMap { get; private set; }
        public Dictionary<int, int> BrokenVehicle { get; private set; } = new();


        private int _actualRoundNumber = 0;

        private Dictionary<int, Queue<VehicleGui>> _dictionaryQueue = new();
        private Logger _logger;

        public static string SimulationFolderName => "Simulation";
        public static string SimulationFolderPath => Path.Combine(Const.ApplicationPath, SimulationFolderName);
        private int CountCheckRound = 0;


        public EnvironmentActor(Dictionary<int, IActorRef> actorsMap)
        {
            _logger = new Logger("Environment");
            ActorsMap = actorsMap;
            if(!Directory.Exists(SimulationFolderPath))
            {
                Directory.CreateDirectory(SimulationFolderPath);
            }

            Receive<PriorityNotification>(message =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (EnvironmentViewModel.Instance.InputVehicles[message.Identifier - 1] is not null)
                        EnvironmentViewModel.Instance.InputVehicles[message.Identifier - 1].Priority = message.Priority;
                });
            });

           

            Receive<NewLeaderNotification>(message =>
            {

                if(_rounds.Count() > 2)
                {
                    CheckRound(_rounds[CountCheckRound++]);
                }
                

                _actualRoundNumber++;
                _logger.LogInformation($"Start round number {_actualRoundNumber}");

                Sender.Tell(new NewLeaderNotificationResponse { RoundNumber = _actualRoundNumber });

                Application.Current.Dispatcher.Invoke(() =>
                {
                    EnvironmentViewModel.Instance.LeaderIdentifier = message.Identifier;
                    EnvironmentViewModel.Instance.ActualRound = _actualRoundNumber;
                });

                _actualRound = new CrossRoundStatusDto();
                _actualRound.Number = _actualRoundNumber;
                _actualRound.LeaderVehicle = message.Identifier;

                foreach(var vehicle in message.InvolvedVehicles)
                {
                    _actualRound.Vehicles.Add(vehicle);
                }

                foreach (var brokenNode in message.BrokenNodes)
                {
                    _actualRound.BrokenNode.Add(brokenNode);
                }

                _rounds.Add(_actualRound);

            });

            Receive<VehicleExitNotification>(message =>
            {
                if (message.BrokenNode.HasValue)
                {
                    EnvironmentViewModel.Instance.CrossVehicles[message.BrokenNode.Value - 1] = null;
                }
                else if (message.InputLane != message.Identifier)
                {
                    ExitVehicleNotification(message.InputLane, message.Identifier, message.ActualRound);
                    _logger.LogInformation($"Vehicle with ID {message.Identifier} exit from cross on round {_actualRoundNumber} and associate to round {message.ActualRound}");
                }
                else
                {
                    _logger.LogInformation($"Vehicle with ID {message.Identifier} move in crossing zone on round {_actualRoundNumber}");
                    if (_dictionaryQueue.ContainsKey(message.InputLane))
                    {
                        var queue = _dictionaryQueue[message.InputLane];
                        if (queue.Count > 0)
                        {
                            var newVehicle = queue.Dequeue();

                            AddNewVehicle(new VehicleDto
                            {
                                InputLane = message.InputLane,
                                OutputLane = newVehicle.OutputLane,
                                Speed = newVehicle.Speed,
                                BrokenNode = newVehicle.BrokenNode,
                                LicensePlate = _licensePlateCount++
                            });

                            EnvironmentViewModel.Instance.RemoveLaneItem(newVehicle);
                            Task.Run(async () =>
                            {
                                Application.Current.Dispatcher.Invoke(() => EnvironmentViewModel.Instance.InputVehicles[message.InputLane - 1] = null);
                                await Task.Delay(500);
                                Application.Current.Dispatcher.Invoke(() => EnvironmentViewModel.Instance.InputVehicles[message.InputLane - 1] = newVehicle);

                            });

                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                                EnvironmentViewModel.Instance.InputVehicles[message.InputLane - 1] = null);
                        }
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                                EnvironmentViewModel.Instance.InputVehicles[message.InputLane - 1] = null);
                    }

                }
            });

            Receive<VehicleMoveNotification>(message =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                    {


                        var vehicle = new VehicleGui(message.Vehicle);
                        EnvironmentViewModel.Instance.TechNodes[message.Vehicle.OutputLane - 1] = message.Vehicle.InputLane;
                        foreach (var croosNode in message.CrossingNodes)
                        {
                            EnvironmentViewModel.Instance.TechNodes[croosNode - 1] = message.Vehicle.InputLane;
                        }

                        EnvironmentViewModel.Instance.OutputVehicles[message.Vehicle.InputLane - 1] = vehicle;

                        if (vehicle.InputLane == 1)
                        {
                            EnvironmentViewModel.Instance.Vehicle1Destination = 0;
                            EnvironmentViewModel.Instance.Vehicle1Destination = vehicle.OutputLane;
                        }

                        if (vehicle.InputLane == 2)
                        {
                            EnvironmentViewModel.Instance.Vehicle2Destination = 0;
                            EnvironmentViewModel.Instance.Vehicle2Destination = vehicle.OutputLane;
                        }

                        if (vehicle.InputLane == 3)
                        {
                            EnvironmentViewModel.Instance.Vehicle3Destination = 0;
                            EnvironmentViewModel.Instance.Vehicle3Destination = vehicle.OutputLane;
                        }

                        if (vehicle.InputLane == 4)
                        {
                            EnvironmentViewModel.Instance.Vehicle4Destination = 0;
                            EnvironmentViewModel.Instance.Vehicle4Destination = vehicle.OutputLane;
                        }
                    });

            });

            Receive<RoundEndNotification>(_ =>
            {
                _logger.LogInformation($"Round {_actualRoundNumber} is end");
            });

            Receive<EnqueueNewVehicle>(message =>
            {
                _logger.LogInformation($"Add new vehicle in the queue. Start lane {message.Vehicle.InputLane} - Exit lane {message.Vehicle.OutputLane}");

                CheckVehicleIsToBroken(message.Vehicle);

                var isAdded = AddNewVehicle(message.Vehicle);
                var newVehicle = new VehicleGui(message.Vehicle);

                if (!isAdded)
                {


                    if (_dictionaryQueue.ContainsKey(newVehicle.InputLane))
                    {
                        _dictionaryQueue[newVehicle.InputLane].Enqueue(newVehicle);
                    }
                    else
                    {
                        var queue = new Queue<VehicleGui>();
                        queue.Enqueue(newVehicle);
                        _dictionaryQueue.Add(newVehicle.InputLane, queue);
                    }

                    EnvironmentViewModel.Instance.AddNewLaneItem(newVehicle.InputLane, newVehicle);


                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() => EnvironmentViewModel.Instance.InputVehicles[newVehicle.InputLane - 1] = newVehicle);
                }
            });


            Receive<VehicleBrokenNotification>(message =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    EnvironmentViewModel.Instance.CrossVehicles[message.Vehicle.BrokenNode.Value - 1] = new VehicleGui(message.Vehicle);
                });

                var thread = new Thread(_ =>
                {
                    var removeTime = _secondForRemoving * 1000;
                    Thread.Sleep(removeTime);
                    var actor = ActorsMap[Const.BrokenIdentifier];
                    actor.Tell(new VehicleBrokenRemoveCommand(message.Vehicle.BrokenNode.Value));
                });
                thread.Start();
            });

            Receive<StopSimulationCommand>(message =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _dictionaryQueue.Clear();
                    foreach (var queue in EnvironmentViewModel.Instance.Queues)
                    {
                        queue.Queue.Clear();
                    }

                });
            });

            Receive<CreateSimulationData>(message =>
            {
                CreateJsonSimulation(_rounds);
                _rounds.Clear();
            });

            Receive<SimulationSettingCommand>(message =>
            {
                _secondForRemoving = message.TimeForRepair;
                _numberVehicleForBroke = message.NumberVehicleForBroke;
            });

        }

        private bool AddNewVehicle(VehicleDto vehicle)
        {
            var response = ActorsMap[vehicle.InputLane].Ask<VehicleOnNodeNotification>(new VehicleOnNodeCommand
            {
                Vehicle = vehicle
            }, Const.MaxTimeout);

            return response.Result.IsAdded;
        }

        private void CheckVehicleIsToBroken(VehicleDto vehicle)
        {
            _numberOfVehiclesSpawned++;
            if(_numberOfVehiclesSpawned % _numberVehicleForBroke == 0)
            {
               

                Random randInput = new Random(Guid.NewGuid().GetHashCode());
                var entryLane = randInput.Next(1, 101);
                if(entryLane > 94)
                {
                    vehicle.BrokenNode = vehicle.InputLane;
                    return;
                }

                var outputLane = randInput.Next(1, 101);
                if (outputLane > 94)
                {
                    vehicle.BrokenNode = vehicle.OutputLane;
                    return;
                }

                var crossLane = randInput.Next(9, 18);
                vehicle.BrokenNode = crossLane;
            }
        }

        private void ExitVehicleNotification(int startLane, int exitLane, int roundNumber)
        {

            var targetRound = _rounds.First(x => x.Number == roundNumber);
            targetRound.VehiclesRunning.Add(startLane);
            EnvironmentViewModel.Instance.OutputVehicles[startLane - 1] = null;
            for (var i = 0; i < EnvironmentViewModel.Instance.TechNodes.Count; i++)
            {
                if (EnvironmentViewModel.Instance.TechNodes[i] == startLane)
                {
                    EnvironmentViewModel.Instance.TechNodes[i] = 0;
                }
            }

        }

        public static Props Props(Dictionary<int, IActorRef> actorsMap)
        {
            return Akka.Actor.Props.Create(() => new EnvironmentActor(actorsMap));
        }

       

        public void CreateJsonSimulation(List<CrossRoundStatusDto> crossRounds)
        {
            var simulationFile = Path.Combine(SimulationFolderPath, $"{DateTime.Now:yyyyMMdd_hhmmss}_Simulation.json");

            // serialize JSON directly to a file
            using (StreamWriter file = File.CreateText(simulationFile))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, crossRounds);
            }
        }

        public void CheckRound(CrossRoundStatusDto crossData)
        {
            if (crossData is null) return;

            _logger.LogInformation($"Check round number {crossData.Number}...");

            var expectedLeader = crossData.Vehicles.Max(x => x.InputLane);
            var leader = crossData.LeaderVehicle;

            if(expectedLeader != leader)
            {
                _logger.LogError($"Leader on round {crossData.Number} is {leader}, but was expected {expectedLeader}");
            }

            var builder = AlgorithmViewModel.Instance.BasicBuilder;
            var crossMap = builder.Build();
            crossData.BrokenNode.ForEach(crossMap.AddBrokenNode);
            crossData.Vehicles.ForEach(crossMap.AddVehicle);
           

            var algorithm = new CollisionAlgorithm(crossMap);
            algorithm.Calculate();
            var vehiclesIdentifier = crossData.Vehicles.Select(x => x.InputLane);
           
            foreach(var vehicleIdentifier in vehiclesIdentifier)
            {
                if(algorithm.AmIRunner(vehicleIdentifier))
                {
                    var vehicleFound = crossData.VehiclesRunning.FirstOrDefault(x => x == vehicleIdentifier);
                    if(vehicleFound is 0)
                    {
                        _logger.LogError($"On round {crossData.Number} a vehicle with identifier {vehicleIdentifier} is not a runner but it expected that run on this round");
                    }
                }
                else
                {
                    var vehicleFound = crossData.VehiclesRunning.FirstOrDefault(x => x == vehicleIdentifier);
                    if (vehicleFound is not 0)
                    {
                        _logger.LogError($"On round {crossData.Number} a vehicle with identifier {vehicleIdentifier} is a runner but it expected that wait for next round");
                    }
                }
            }


           

           
        }
    }
}
