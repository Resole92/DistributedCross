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
    public class NodeActor : ReceiveActor, IWithUnboundedStash
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

            if (Identifier == 0) return;

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
                if (Vehicle is not null)
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
                        if (x.IsCanceled || x.IsFaulted)
                            return new ElectionResult
                            {
                                IsFailed = false,
                            };

                        return x.Result;
                      
                    }, TaskContinuationOptions.ExecuteSynchronously)
                    .PipeTo(self);


                }
            });

            Receive<ElectionStart>(message =>
            {
                if (Vehicle is not null)
                {
                    _logger.LogInformation("An election is start...");
                    Self.Tell(new LeaderElectionRequest(), Self);
                }
            });

            Receive<LeaderNotificationRequest>(message =>
            {
                if (Vehicle is not null)
                {
                   
                    var response = Vehicle.LeaderElected(message);
                    if (response.Acknowledge)
                    {
                        _logger.LogInformation($"A leader with ID {message.Identifier} is elected");
                    }
                    else
                        _logger.LogInformation($"A leader with ID {message.Identifier} is refused");

                    Sender.Tell(response, Self);
                   
                }
            });

            Receive<VehicleRemoveNotification>(message =>
            {
                if (Vehicle is null) return;
                Vehicle.RemoveParentNode();
                Vehicle = null;

                Become(IdleBehaviour);
            });

            Receive<CoordinationNotificationRequest>(message =>
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
                        Become(EntryBehaviour);
                    }

                }
            });

            Receive<VehicleExitNotification>(message =>
            {
                if (Vehicle is not null)
                {
                    Vehicle.VehicleExit(message.Identifier);
                }
            });


            BaseBehaviour();
        }

        #endregion

        #region Election behaviour

        public void ElectionBehaviour()
        {

            Receive<ElectionStart>(message =>
            {
                if (Vehicle is not null)
                {
                    _logger.LogInformation("An election is already started...");
                }
            });


            Receive<LeaderElectionRequest>(message =>
            {
                if (Vehicle is not null)
                {
                   
                    Sender.Tell(new LeaderElectionResponse
                    {
                        Identifier = Identifier
                    }, Self);
                }
            });

            Receive<LeaderNotificationRequest>(message =>
            {
                if (Vehicle is not null)
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
                }
            });

            Receive<CoordinationNotificationRequest>(message =>
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
                        Become(EntryBehaviour);
                    }

                }
            });

            Receive<VehicleRemoveNotification>(message =>
            {
                _logger.LogInformation($"Message removing is stashed");
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
                if (Vehicle is not null)
                {
                    _logger.LogInformation($"An election start is refused because i'm coordination behaviour");
                }
            });

            Receive<LeaderElectionRequest>(message =>
            {
                if (Vehicle is not null)
                {
                    Sender.Tell(new LeaderElectionResponse
                    {
                        Identifier = Identifier
                    });
                    _logger.LogInformation($"A election leader request is refused because I'm coordination behaviour");
                }
            });

            Receive<LeaderNotificationRequest>(message =>
            {
                if (Vehicle is not null)
                {
                    _logger.LogInformation($"An new leader is refused from node because I'm coordination behaviour");
                    Sender.Tell(new LeaderNotificationResponse
                    {
                        Acknowledge = false
                    });
                }
            });

            Receive<VehicleRemoveNotification>(message =>
            {
                if (Vehicle is null) return;
                _logger.LogInformation($"Vehicle is removed");
                Vehicle.RemoveParentNode();
                Vehicle = null;

                Become(IdleBehaviour);
            });

            Receive<VehicleExitNotification>(message =>
            {
                if (Vehicle is not null)
                {
                    Vehicle.VehicleExit(message.Identifier);
                }
            });

            BaseBehaviour();
        }


        #endregion

        #region CrossingBehaviour


        private void CrossingBehaviour()
        {
            Receive<ElectionStart>(message =>
            {
                if (Vehicle is not null)
                {
                    _logger.LogInformation($"An election start is refused because I'm crossing behaviour");
                }
            });

            Receive<LeaderElectionRequest>(message =>
            {
                if (Vehicle is not null)
                {
                    Sender.Tell(new LeaderElectionResponse
                    {
                        Identifier = Identifier
                    }, Self);
                    _logger.LogInformation($"A leader election is refused because I'm crossing behaviour");
                }
            });

            Receive<LeaderNotificationRequest>(message =>
            {
                if (Vehicle is not null)
                {
                    _logger.LogInformation($"An new leader is refused from node because I'm in crossing behaviour");
                    Sender.Tell(new LeaderNotificationResponse());
                }
            });


            Receive<VehicleRemoveNotification>(message =>
            {
                if (Vehicle is null) return;
                _logger.LogInformation($"Vehicle is removed");
                Vehicle.RemoveParentNode();
                Vehicle = null;

                Become(IdleBehaviour);
            });

            Receive<VehicleExitNotification>(message =>
            {
                if (Vehicle is not null)
                {
                    Vehicle.VehicleExit(message.Identifier);
                }
            });

            BaseBehaviour();


        }

        #endregion

        #region Base behaviour

        private void BaseBehaviour()
        {
          
            

            Receive<InformationVehicleRequest>(messsage =>
            {
                if (Vehicle is not null)
                {
                    Sender.Tell(new InformationVehicleResponse
                    {
                        Vehicle = Vehicle.Data
                    });
                }
            });

            Receive<ElectionResult>(message =>
            {
                Stash.UnstashAll();

                if (message.IsFailed)
                {
                    _logger.LogInformation("An election is failed");
                    Become(EntryBehaviour);
                }
                else
                {
                    _logger.LogInformation("An election is conclude successfully");
                }

            });

        }

        #endregion

        #region Idle behaviour

        public void IdleBehaviour()
        {
            Receive<VehicleOnNodeNotification>(message =>
            {
                _logger.LogInformation($"A new vehicle enter on cross");
                Vehicle = new Vehicle(message.Vehicle, _builder, this, _logger);
                Self.Tell(new ElectionStart());
                Become(EntryBehaviour);
            });

            Receive<VehicleMoveNotification>(message =>
            {
                _logger.LogInformation($"A new vehicle is crossing into this lane");
                Vehicle = new Vehicle(message.Vehicle, _builder, this, _logger);
                Vehicle.UpdateCrossingStatus(message);
                Vehicle.StartCrossing();
                Become(CrossingBehaviour);
            });

        }

        #endregion

        public static Props Props(int identifier, CrossBuilder builder, Dictionary<int, IActorRef> actorsMap)
        {
            return Akka.Actor.Props.Create(() => new NodeActor(identifier, builder, actorsMap));
        }


    }
}
