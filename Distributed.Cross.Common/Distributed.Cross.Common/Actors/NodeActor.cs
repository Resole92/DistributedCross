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

        private Logger _logger = new Logger();
        private CrossBuilder _builder;
        private CrossMap _map;
        private CancellationTokenSource _tokenAsk;


        public NodeActor(int identifier, CrossBuilder builder, Dictionary<int, IActorRef> actorsMap)
        {
            Identifier = identifier;
            ActorsMap = actorsMap;
            _builder = builder;

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
                    Become(ElectionBehaviour);
                    _tokenAsk = new CancellationTokenSource();

                    var self = Self;
                    var vehicle = Vehicle;

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

                    //ReceiveAny(o => Stash.Stash());
                   
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
                    _logger.LogInformation($"A leader is elected with id {message.Identifier}");
                    var data = Vehicle.LeaderElected(message);
                    Sender.Tell(new LeaderNotificationResponse
                    {
                        VehicleDetail = data
                    }, Self);
                }
            });

            Receive<VehicleRemoveNotification>(message =>
            {
                Stash.Stash();
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
                    _logger.LogInformation($"A node  {message.Identifier} want to be a leader but i'm a node with bigger identifier {Identifier}");
                }
            });

            Receive<LeaderNotificationRequest>(message =>
            {
                if (Vehicle is not null)
                {
                    _logger.LogInformation($"A leader is elected with id {message.Identifier}");
                    var data = Vehicle.LeaderElected(message);
                    Sender.Tell(new LeaderNotificationResponse
                    {
                        VehicleDetail = data
                    }, Self);
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
                    _logger.LogInformation($"An election start is refused from node {Identifier} because is in coordination behaviour");
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
                    _logger.LogInformation($"A election leader request is refused from node {Identifier} because is in coordination behaviour");
                }
            });

            Receive<LeaderNotificationRequest>(message =>
            {
                if (Vehicle is not null)
                {
                    _logger.LogInformation($"An new leader is refused from node {Identifier} because is in coordination behaviour");
                    Sender.Tell(new LeaderNotificationResponse
                    {
                        Acknowledge = false
                    });
                }
            });

            Receive<VehicleRemoveNotification>(message =>
            {
                if (Vehicle is null) return;
                Vehicle.RemoveParentNode();
                Vehicle = null;

                Become(IdleBehaviour);
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
                    _logger.LogInformation($"An election start is refused from node {Identifier} because is in crossing behaviour");
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
                    _logger.LogInformation($"A leader election is refused from node {Identifier} because is in crossing behaviour");
                }
            });

            Receive<LeaderNotificationRequest>(message =>
            {
                if (Vehicle is not null)
                {
                    _logger.LogInformation($"An new leader is refused from node {Identifier} because is in crossing behaviour");
                    Sender.Tell(new LeaderNotificationResponse());
                }
            });


            Receive<VehicleRemoveNotification>(message =>
            {
                if (Vehicle is null) return;
                Vehicle.RemoveParentNode();
                Vehicle = null;

                Become(IdleBehaviour);
            });


            BaseBehaviour();


        }

        #endregion

        #region Base behaviour

        private void BaseBehaviour()
        {
          
            Receive<VehicleExitNotification>(message =>
            {
                if (Vehicle is not null)
                {
                    Vehicle.VehicleExit(message.Identifier);
                }
            });

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
                if (message.IsFailed)
                {
                    Become(EntryBehaviour);
                }

                Stash.UnstashAll();

            });

        }

        #endregion 


        public void IdleBehaviour()
        {
            Receive<VehicleOnNodeNotification>(message =>
            {
                _logger.LogInformation($"A new vehicle {Identifier} enter on cross");
                Vehicle = new Vehicle(message.Vehicle, _builder, this);
                Self.Tell(new ElectionStart());
                Become(EntryBehaviour);
            });

            Receive<VehicleMoveNotification>(message =>
            {
                _logger.LogInformation($"A new vehicle is crossing to lane {Identifier}");
                Vehicle = new Vehicle(message.Vehicle, _builder, this);
                Vehicle.UpdateCrossingStatus(message);
                Vehicle.StartCrossing();
                Become(CrossingBehaviour);
            });



        }


        public static Props Props(int identifier, CrossBuilder builder, Dictionary<int, IActorRef> actorsMap)
        {
            return Akka.Actor.Props.Create(() => new NodeActor(identifier, builder, actorsMap));
        }


    }
}
