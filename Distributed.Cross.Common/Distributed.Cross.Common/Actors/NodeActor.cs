using Akka.Actor;
using Distributed.Cross.Common.Communication.Messages;
using Distributed.Cross.Common.Module;
using Distributed.Cross.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Distributed.Cross.Common.Actors
{
    public partial class NodeActor : ReceiveActor, IWithUnboundedStash
    {

        public Dictionary<int, IActorRef> ActorsMap { get; private set; }
        public int Identifier { get; private set; }
        public Vehicle Vehicle { get; set; }
        public IStash Stash { get; set; }

        private Logger _logger;
        private CrossBuilder _builder;
        private CrossMap _map;
        private CancellationTokenSource _tokenAsk;


        public NodeActor(int identifier, CrossBuilder builder, Dictionary<int, IActorRef> actorsMap)
        {

            Identifier = identifier;
            ActorsMap = actorsMap;
            _builder = builder;
            _logger = new Logger($"Vehicle {Identifier}");

            BuildMap(builder);

            IdleBehaviour();

        }

        private void BuildMap(CrossBuilder builder)
        {
            builder.CreateBasicInputOutput();
            var crossMap = builder.Build();
            _map = crossMap;
        }

        #region Entry behaviour (Request election)

        public void EntryBehaviour()
        {

            Receive<LeaderElectionRequest>(message =>
            {

                Sender.Tell(new LeaderElectionResponse
                {
                    Identifier = Identifier
                }, Self);
                _tokenAsk = new CancellationTokenSource();

                var self = Self;
                var vehicle = Vehicle;

                Become(ElectionBehaviour);

                Task.Run(() =>
                    vehicle.LeaderRequestAsk(_tokenAsk.Token)
                    , _tokenAsk.Token)
                .ContinueWith(x =>
                {
                    if (x.IsCanceled) return new ElectionResult(ElectionResultType.Cancelled);
                    if (x.IsFaulted) return new ElectionResult(ElectionResultType.NotHandled);

                    return x.Result;

                }, TaskContinuationOptions.ExecuteSynchronously)
                .PipeTo(self);

            });

            Receive<ElectionStart>(message =>
            {
                _logger.LogInformation("An election is start...");
                Self.Tell(new LeaderElectionRequest(), Self);
            });

            Receive<LeaderNotificationRequest>(message =>
            {
                var response = Vehicle.LeaderElected(message);
                if (response.Acknowledge)
                {
                    _logger.LogInformation($"A leader with ID {message.Identifier} is elected");
                }
                else
                    _logger.LogInformation($"A leader with ID {message.Identifier} is refused");

                Sender.Tell(response, Self);
            });

            Receive<VehicleRemoveCommand>(RemoveVehicle);
            Receive<RoundEndNotification>(Vehicle.EndRound);
            Receive<CoordinationNotification>(CheckRunner);
            Receive<VehicleExitNotification>(Vehicle.CheckEndRound);


            BaseBehaviour();
        }

        #endregion

        #region Election behaviour

        public void ElectionBehaviour()
        {

            Receive<ElectionStart>(message =>
            {
                _logger.LogInformation($"Election is stashed");
                Stash.Stash();
            });


            Receive<LeaderElectionRequest>(message =>
            {
                Sender.Tell(new LeaderElectionResponse
                {
                    Identifier = Identifier
                }, Self);
            });

            Receive<LeaderNotificationRequest>(message =>
            {
                var response = Vehicle.LeaderElected(message);

                if (response.Acknowledge)
                {
                    _tokenAsk.Cancel();
                    _logger.LogInformation($"A leader with ID {message.Identifier} is elected");
                }
                else
                {
                    _logger.LogInformation($"A leader with ID {message.Identifier} is refused");
                }

                Sender.Tell(response, Self);
            });

            Receive<CoordinationNotification>(CheckRunner);

            Receive<VehicleRemoveCommand>(message =>
            {
                _logger.LogInformation($"Message removing is stashed");
                Stash.Stash();
            });

            Receive<RoundEndNotification>(message =>
            {
                _logger.LogInformation($"Message end is stashed");
                Stash.Stash();
            });

            Receive<VehicleExitNotification>(message =>
            {
                _logger.LogInformation($"Message exit is stashed");
                Stash.Stash();
            });

            BaseBehaviour();
        }

        #endregion

        #region Coordination behaviour

        public void CoordinationBehaviour()
        {

            Receive<ElectionStart>(message =>
            {
                _logger.LogInformation($"An election start is refused because i'm coordination behaviour");
            });

            Receive<LeaderElectionRequest>(message =>
            {
                Sender.Tell(new LeaderElectionResponse
                {
                    Identifier = Identifier
                });
                _logger.LogInformation($"A election leader request is refused because I'm coordination behaviour");
            });

            Receive<LeaderNotificationRequest>(message =>
            {
                _logger.LogInformation($"An new leader is refused from node because I'm coordination behaviour");
                Sender.Tell(new LeaderNotificationResponse
                {
                    Acknowledge = false
                });
            });

            Receive<VehicleRemoveCommand>(RemoveVehicle);
            Receive<RoundEndNotification>(Vehicle.EndRound);
            Receive<VehicleExitNotification>(Vehicle.CheckEndRound);

            BaseBehaviour();
        }


        #endregion

        #region CrossingBehaviour


        private void CrossingBehaviour()
        {
            Receive<ElectionStart>(message =>
            {
                _logger.LogInformation($"An election start is refused because I'm crossing behaviour");
            });

            Receive<LeaderElectionRequest>(message =>
            {
                Sender.Tell(new LeaderElectionResponse
                {
                    Identifier = Identifier
                }, Self);
                _logger.LogInformation($"A leader election is refused because I'm crossing behaviour");

            });

            Receive<LeaderNotificationRequest>(message =>
            {
                _logger.LogInformation($"An new leader is refused from node because I'm in crossing behaviour");
                Sender.Tell(new LeaderNotificationResponse());

            });

            Receive<VehicleRemoveCommand>(RemoveCrossingVehicle);
            Receive<RoundEndNotification>(Vehicle.EndRound);
            Receive<VehicleExitNotification>(Vehicle.CheckEndRound);

            BaseBehaviour();


        }

        #endregion

        #region Idle behaviour

        public void IdleBehaviour()
        {


            Receive<VehicleOnNodeCommand>(message =>
            {
                _logger.LogInformation($"A new vehicle enter on cross");
                Vehicle = new Vehicle(message.Vehicle, _builder, this, _logger);
                Self.Tell(new ElectionStart());
                var notification = new VehicleOnNodeNotification
                {
                    Identifier = Identifier,
                    IsAdded = true
                };
                Sender.Tell(notification);

                Become(EntryBehaviour);

            });

            Receive<VehicleMoveCommand>(message =>
            {
                _logger.LogInformation($"A new vehicle is crossing into this lane");
                Vehicle = new Vehicle(message.Vehicle, _builder, this, _logger);
                Vehicle.UpdateCrossingStatus(message);
                Vehicle.StartCrossing();
                SendBroadcastMessage(new VehicleMoveNotification(message.Vehicle, message.CrossNode));

                Become(CrossingBehaviour);
            });


            Receive<VehicleBrokenCommand>(message =>
            {
                Self.Tell(message);
                Become(BrokenBehaviour);
            });


        }

        #endregion

        #region Base behaviour

        private void BaseBehaviour()
        {


            Receive<InformationVehicleRequest>(messsage =>
            {
                Sender.Tell(new InformationVehicleResponse
                {
                    Vehicle = Vehicle.Data
                });
            });

            Receive<ElectionResult>(message =>
            {
                Stash.UnstashAll();

                switch (message.Result)
                {
                    case ElectionResultType.Cancelled:
                        {
                            _logger.LogInformation("An election is cancelled");
                            break;
                        }
                    case ElectionResultType.Bully:
                        {
                            _logger.LogInformation("I'm bullied from another vehicle");
                            Become(EntryBehaviour);
                            break;
                        }
                    case ElectionResultType.Crossing:
                        {
                            _logger.LogInformation("Other vehicle are crossing...");
                            Become(EntryBehaviour);
                            break;
                        }
                    case ElectionResultType.NotHandled:
                        {
                            _logger.LogInformation("Some error is not handled during leader process");
                            Become(EntryBehaviour);
                            break;
                        }
                    case ElectionResultType.LeaderAlreadyPresent:
                        {
                            _logger.LogInformation("Another leader is present");
                            Become(EntryBehaviour);
                            break;
                        }
                    case ElectionResultType.Elected:
                        {
                            _logger.LogInformation("An election is conclude successfully. I'm elected");
                            break;
                        }
                }

            });

            Receive<VehicleOnNodeCommand>(message =>
            {
                _logger.LogInformation("Not possible add another vehicle because already one is in this node");
                Sender.Tell(new VehicleOnNodeNotification { IsAdded = false });
            });

        }

        #endregion

        public void CheckIfBroken()
        {
            if (Vehicle.Data.BrokenNode.HasValue)
            {
                var brokeNode = ActorsMap[Const.BrokenIdentifier];
                brokeNode.Tell(new VehicleBrokenCommand(Vehicle.Data), Self);
            }
           
        }

        private void CheckRunner(CoordinationNotification message)
        {
            if (Vehicle is not null)
            {
                _tokenAsk.Cancel();
                _logger.LogInformation($"Coordination data received!");
                var isRunner = Vehicle.CoordinationInformationReceive(message);
                if (isRunner)
                {
                    Become(CoordinationBehaviour);
                }
                else
                {
                    SendBroadcastMessage(new PriorityNotification(Identifier, Vehicle.Data.Priority));
                    Become(EntryBehaviour);
                }
            }
        }

        public void RemoveCrossingVehicle(VehicleRemoveCommand message)
        {

            CheckIfBroken();
            RemoveVehicle(message);
        }

        public void RemoveVehicle(VehicleRemoveCommand message)
        {
            var startLane = Vehicle.Data.InputLane;
            var roundNumber = Vehicle.ActualRound;
            _logger.LogInformation($"Vehicle is removed");
            Vehicle.RemoveParentNode();
            Vehicle = null;

            var exitMessage = new VehicleExitNotification
            {
                InputLane = startLane,
                Identifier = Identifier,
                BrokenNode = message.BrokenNode,
                ActualRound = roundNumber

            };
            SendBroadcastMessage(exitMessage);

            Become(IdleBehaviour);
        }

        public void SendBroadcastMessage(object message)
        {
            var selection = Context.ActorSelection("akka://MySystem/user/*");
            selection.Tell(message, Self);
        }

        public static Props Props(int identifier, CrossBuilder builder, Dictionary<int, IActorRef> actorsMap)
        {
            return Akka.Actor.Props.Create(() => new NodeActor(identifier, builder, actorsMap));
        }


    }
}
