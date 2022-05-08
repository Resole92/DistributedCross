using Akka.Actor;
using Distributed.Cross.Common.Communication.Environment;
using Distributed.Cross.Common.Communication.Messages;
using Distributed.Cross.Common.Data;
using Distributed.Cross.Common.Module;
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

        private int _exampleToSelect;
        private Dictionary<int, Queue<QueueItem>> _dictionaryQueue = new();
        private Logger _logger;

        public EnvironmentActor(Dictionary<int, IActorRef> actorsMap)
        {
            _logger = new Logger("Environment");
            ActorsMap = actorsMap;

            Receive<ElectionStart>(message =>
            {
                //EnvironmentViewModel.Instance.SelectedExample = _exampleToSelect % 2;
                //EnvironmentViewModel.Instance.StartEnvironmentCommand?.Execute(null);
                //_exampleToSelect++;
                _actualRound++;
            });

            Receive<VehicleExitNotification>(message =>
            {
                if (message.StartLane != message.Identifier)
                {
                    ExitVehicleNotification(message.StartLane, message.Identifier);
                    _logger.LogInformation($"Vehicle with ID {message.Identifier} exit from cross");
                }
                else
                {
                    _logger.LogInformation($"Vehicle with ID {message.Identifier} move in crossing zone");
                    if (_dictionaryQueue.ContainsKey(message.StartLane))
                    {
                        var queue = _dictionaryQueue[message.StartLane];
                        if (queue.Count > 0)
                        {
                            var newVehicle = queue.Dequeue();
                            AddNewVehicle(new VehicleDto
                            {
                                InputLane = message.StartLane,
                                OutputLane = newVehicle.EndLane,
                            });



                            EnvironmentViewModel.Instance.RemoveLaneItem(newVehicle);
                            Task.Run(async () =>
                            {
                                EnvironmentViewModel.Instance.InputVehicles[message.StartLane - 1] = null;
                                await Task.Delay(500);
                                EnvironmentViewModel.Instance.InputVehicles[message.StartLane - 1] = new VehicleGui
                                {
                                    Priority = 1,
                                    StartLane = message.StartLane,
                                    EndLane = newVehicle.EndLane,
                                };
                            });

                        }
                        else
                        {

                            EnvironmentViewModel.Instance.InputVehicles[message.StartLane - 1] = null;
                        }
                    }

                }
            });

            Receive<VehicleMoveNotification>(message =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                    {

                        EnvironmentViewModel.Instance.OutputVehicles[message.InputLane - 1] = new VehicleGui
                        {
                            StartLane = message.InputLane,
                            EndLane = message.OutputLane
                        };

                        if (message.InputLane == 1)
                        {
                            EnvironmentViewModel.Instance.Vehicle1Destination = 0;
                            EnvironmentViewModel.Instance.Vehicle1Destination = message.OutputLane;
                        }

                        if (message.InputLane == 2)
                        {
                            EnvironmentViewModel.Instance.Vehicle2Destination = 0;
                            EnvironmentViewModel.Instance.Vehicle2Destination = message.OutputLane;
                        }

                        if (message.InputLane == 3)
                        {
                            EnvironmentViewModel.Instance.Vehicle3Destination = 0;
                            EnvironmentViewModel.Instance.Vehicle3Destination = message.OutputLane;
                        }

                        if (message.InputLane == 4)
                        {
                            EnvironmentViewModel.Instance.Vehicle4Destination = 0;
                            EnvironmentViewModel.Instance.Vehicle4Destination = message.OutputLane;
                        }
                    });

            });

            Receive<EnqueueNewVehicle>(message =>
            {
                _logger.LogInformation($"Add new vehicle in the queue. Start lane {message.StartLane} - Exit lane {message.DestinationLane}");

                var isAdded = AddNewVehicle(new VehicleDto
                {
                    InputLane = message.StartLane,
                    OutputLane = message.DestinationLane
                });

                if (!isAdded)
                {
                    var newItem = new QueueItem
                    {
                        EndLane = message.DestinationLane
                    };

                    if (_dictionaryQueue.ContainsKey(message.StartLane))
                    {
                        _dictionaryQueue[message.StartLane].Enqueue(newItem);
                    }
                    else
                    {
                        var queue = new Queue<QueueItem>();
                        queue.Enqueue(newItem);
                        _dictionaryQueue.Add(message.StartLane, queue);
                    }

                    EnvironmentViewModel.Instance.AddNewLaneItem(message.StartLane, newItem);


                    EnvironmentViewModel.Instance.InputVehicles[message.StartLane - 1] = new VehicleGui
                    {
                        Priority = 1,
                        StartLane = message.StartLane,
                        EndLane = message.DestinationLane,
                    };

                }

            });
        }

        private bool AddNewVehicle(VehicleDto vehicle)
        {
            var response = ActorsMap[vehicle.InputLane].Ask<VehicleOnNodeNotification>(new VehicleOnNodeCommand
            {
                Vehicle = vehicle
            }, TimeSpan.FromSeconds(1.5));

            return response.Result.IsAdded;
        }

        private void ExitVehicleNotification(int startLane, int exitLane)
        {
            EnvironmentViewModel.Instance.OutputVehicles[startLane - 1] = null;
        }

        public static Props Props(Dictionary<int, IActorRef> actorsMap)
        {
            return Akka.Actor.Props.Create(() => new EnvironmentActor(actorsMap));
        }

    }
}
