using Akka.Actor;
using Distributed.Cross.Common.Communication.Environment;
using Distributed.Cross.Common.Communication.Messages;
using Distributed.Cross.Common.Module;
using Distributed.Cross.Gui.Simulation.Environment;
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

        private int _exampleToSelect;
        private Dictionary<int, Queue<EnqueueNewVehicle>> _dictionaryQueue = new();
        private Logger _logger;

        public EnvironmentActor()
        {
            _logger = new Logger("Environment");

            Receive<ElectionStart>(message =>
            {
                //foreach (var vehicle in message.LastRoundVehicleRunning)
                //{
                //    var findVehicle = message.Vehicles.Where(x => x.DestinationLane == vehicle).OrderByDescending(x => x.Priority).FirstOrDefault();
                //    if (findVehicle is not null)
                //    {
                //        ExitVehicleNotification(findVehicle.StartLane, findVehicle.DestinationLane);
                //    }
                //}

                EnvironmentViewModel.Instance.SelectedExample = _exampleToSelect % 2;
                EnvironmentViewModel.Instance.StartEnvironmentCommand?.Execute(null);
                _exampleToSelect++;
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
                }
                   
            });

            Receive<EnqueueNewVehicle>(message =>
            {
                var response = ActorsMap[message.StartLane].Ask<VehicleOnNodeResponse>(new VehicleOnNodeRequest
                {
                    Vehicle = new Data.VehicleDto
                    {
                        StartLane = message.StartLane,
                        DestinationLane = message.DestinationLane
                    }
                }, TimeSpan.FromSeconds(2));

                if (!response.Result.IsAdded)
                {
                    if (_dictionaryQueue.ContainsKey(message.StartLane))
                    {
                        _dictionaryQueue[message.StartLane].Enqueue(message);
                    }
                    else
                    {
                        var queue = new Queue<EnqueueNewVehicle>();
                        queue.Enqueue(message);
                        _dictionaryQueue.Add(message.StartLane, queue);
                    }

                }
            });
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

    }
}
