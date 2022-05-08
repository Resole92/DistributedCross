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
                if(message.StartLane != message.Identifier)
                {
                    ExitVehicleNotification(message.StartLane, message.Identifier);
                    _logger.LogInformation($"Vehicle with ID {message.Identifier} exit from cross");
                }
                else
                {
                    _logger.LogInformation($"Vehicle with ID {message.Identifier} move in crossing zone");
                    if(_dictionaryQueue.ContainsKey(message.StartLane))
                    {
                        var queue = _dictionaryQueue[message.StartLane];
                        if(queue.Count > 0)
                        {
                            var newVehicle = queue.Dequeue();
                            AddNewVehicle(new VehicleDto
                            {
                                StartLane = message.StartLane,                              
                                DestinationLane = newVehicle.EndLane,
                            });

                            EnvironmentViewModel.Instance.RemoveLaneItem(newVehicle);
                        }
                    }
                }
            });

            Receive<EnqueueNewVehicle>(message =>
            {
                _logger.LogInformation($"Add new vehicle in the queue. Start lane {message.StartLane} - Exit lane {message.DestinationLane}");

                var isAdded = AddNewVehicle(new VehicleDto
                {
                    StartLane = message.StartLane,
                    DestinationLane = message.DestinationLane
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

                }

            });
        }

        private bool AddNewVehicle(VehicleDto vehicle)
        {
            var response = ActorsMap[vehicle.StartLane].Ask<VehicleOnNodeResponse>(new VehicleOnNodeRequest
            {
                Vehicle = vehicle
            }, TimeSpan.FromSeconds(1.5));

            return response.Result.IsAdded;
        }

        private void ExitVehicleNotification(int startLane, int exitLane)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (startLane == 1)
                {
                    EnvironmentViewModel.Instance.Vehicle1Destination = 0;
                    EnvironmentViewModel.Instance.Vehicle1Destination = exitLane;
                }

                if (startLane == 2)
                {
                    EnvironmentViewModel.Instance.Vehicle2Destination = 0;
                    EnvironmentViewModel.Instance.Vehicle2Destination = exitLane;
                }

                if (startLane == 3)
                {
                    EnvironmentViewModel.Instance.Vehicle3Destination = 0;
                    EnvironmentViewModel.Instance.Vehicle3Destination = exitLane;
                }

                if (startLane == 4)
                {
                    EnvironmentViewModel.Instance.Vehicle4Destination = 0;
                    EnvironmentViewModel.Instance.Vehicle4Destination = exitLane;
                }
            });
        }

        public static Props Props(Dictionary<int, IActorRef> actorsMap)
        {
            return Akka.Actor.Props.Create(() => new EnvironmentActor(actorsMap));
        }

    }
}
