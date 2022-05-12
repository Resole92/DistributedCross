using Akka.Actor;
using Distributed.Cross.Common.Communication.Environment;
using Distributed.Cross.Common.Communication.Messages;
using Distributed.Cross.Common.Data;
using Distributed.Cross.Common.Module;
using Distributed.Cross.Common.Utilities;
using Distributed.Cross.Gui.Simulation.Environment;
using Distributed.Cross.Gui.Simulation.Environment.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Distributed.Cross.Common.Actors
{
    public class EnvironmentActor : ReceiveActor
    {
        public Dictionary<int, IActorRef> ActorsMap { get; private set; }

        private int _actualRound = 1;

        private Dictionary<int, Queue<VehicleGui>> _dictionaryQueue = new();
        private Logger _logger;

        public EnvironmentActor(Dictionary<int, IActorRef> actorsMap)
        {
            _logger = new Logger("Environment");
            ActorsMap = actorsMap;

            Receive<PriorityNotification>(message =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (EnvironmentViewModel.Instance.InputVehicles[message.Identifier - 1] is not null)
                        EnvironmentViewModel.Instance.InputVehicles[message.Identifier - 1].Priority = message.Priority;
                });
            });

            Receive<ElectionStart>(message =>
            {
                _actualRound++;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    EnvironmentViewModel.Instance.ActualRound = _actualRound;
                });
            });

            Receive<NewLeaderNotification>(message =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    EnvironmentViewModel.Instance.LeaderIdentifier = message.Identifier;
                });
            });

            Receive<VehicleExitNotification>(message =>
            {
                if(message.BrokenNode.HasValue)
                {
                    EnvironmentViewModel.Instance.CrossVehicles[message.BrokenNode.Value - 9] = null;
                }
                else if (message.InputLane != message.Identifier)
                {
                    ExitVehicleNotification(message.InputLane, message.Identifier);
                    _logger.LogInformation($"Vehicle with ID {message.Identifier} exit from cross");
                }
                else
                {
                    _logger.LogInformation($"Vehicle with ID {message.Identifier} move in crossing zone");
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
                                Speed = newVehicle.Speed
                            });

                            EnvironmentViewModel.Instance.RemoveLaneItem(newVehicle);
                            Task.Run(async () =>
                            {
                                EnvironmentViewModel.Instance.InputVehicles[message.InputLane - 1] = null;
                                await Task.Delay(500);
                                EnvironmentViewModel.Instance.InputVehicles[message.InputLane - 1] = newVehicle;
                            });

                        }
                        else
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                                EnvironmentViewModel.Instance.InputVehicles[message.InputLane - 1] = null);
                        }
                    }

                }
            });

            Receive<VehicleMoveNotification>(message =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                    {
                       

                        var vehicle = new VehicleGui(message.Vehicle);

                        foreach(var croosNode in message.CrossingNodes)
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

            Receive<EnqueueNewVehicle>(message =>
            {
                _logger.LogInformation($"Add new vehicle in the queue. Start lane {message.Vehicle.InputLane} - Exit lane {message.Vehicle.OutputLane}");


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
                    EnvironmentViewModel.Instance.CrossVehicles[message.Vehicle.BrokenNode.Value - 9] = new VehicleGui(message.Vehicle);
                    EnvironmentViewModel.Instance.InputVehicles[message.Vehicle.InputLane - 1] = null;
                });
            });

        }

        private bool AddNewVehicle(VehicleDto vehicle)
        {
            var response = ActorsMap[vehicle.InputLane].Ask<VehicleOnNodeNotification>(new VehicleOnNodeCommand
            {
                Vehicle = vehicle
            }, TimeSpan.FromSeconds(Const.MaxTimeout));

            return response.Result.IsAdded;
        }

        private void ExitVehicleNotification(int startLane, int exitLane)
        {
            EnvironmentViewModel.Instance.OutputVehicles[startLane - 1] = null;
            for(var i = 0; i < EnvironmentViewModel.Instance.TechNodes.Count; i++)
            {
                if(EnvironmentViewModel.Instance.TechNodes[i] == startLane)
                {
                    EnvironmentViewModel.Instance.TechNodes[i] = 0;
                }
            }
          
        }

        public static Props Props(Dictionary<int, IActorRef> actorsMap)
        {
            return Akka.Actor.Props.Create(() => new EnvironmentActor(actorsMap));
        }

    }
}
