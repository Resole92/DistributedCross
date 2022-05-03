using Akka.Actor;
using Distributed.Cross.Common.Communication.Messages;
using Distributed.Cross.Common.Module;
using Distributed.Cross.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distributed.Cross.Common.Actors
{
    public class NodeActor : ReceiveActor
    {

        public Dictionary<int, IActorRef> ActorsMap { get; private set; }
        public int Identifier { get; private set; }
        public Vehicle Vehicle { get; set; }
        private Logger _logger = new Logger();
        private CrossBuilder _builder;
        private CrossMap _map;



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
                    Sender.Tell(new LeaderElectionResponse(), Self);
                    Become(ElectionBehaviour);
                    Task.Run(() =>
                    {
                        Vehicle?.LeaderRequestAsk();
                    });//.PipeTo(self);

                    //ReceiveAny(o => Stash.Stash());
                   
                }
            });

            Receive<ElectionStart>(message =>
            {
                if (Vehicle is not null)
                {
                    _logger.LogInformation("An election is start...");
                    Task.Run(() =>
                    {
                        Vehicle.LeaderRequestAsk();
                    });

                    Become(ElectionBehaviour);
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
                    Sender.Tell(new LeaderElectionResponse(), Self);
                    _logger.LogInformation($"A node  {message.Identifier} want to be a leader but i'm a node with bigger identifier {Identifier}");
                }
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
                    Sender.Tell(new LeaderElectionResponse());
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
                    Sender.Tell(new LeaderElectionResponse(), Self);
                    _logger.LogInformation($"A leader election is refused from node {Identifier} because is in crossing behaviour");
                }
            });

            Receive<LeaderNotificationRequest>(message =>
            {
                if (Vehicle is not null)
                {
                    _logger.LogInformation($"An new leader is refused from node {Identifier} because is in crossing behaviour");
                    Sender.Tell(new LeaderNotificationResponse
                    {
                        Acknowledge = false
                    });
                }
            });

            BaseBehaviour();


        }

        #endregion

        private void BaseBehaviour()
        {
          

            Receive<VehicleExitNotification>(message =>
            {
                if (Vehicle is not null)
                {
                    Vehicle.VehicleExit(message.Identifier);
                }
            });

           

            Receive<VehicleRemoveNotification>(message =>
            {
                if (Vehicle is null) return;
                Vehicle.RemoveParentNode();
                Vehicle = null;

                Become(IdleBehaviour);


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
                    _logger.LogInformation($"Coordination data received!");
                    var isRunner = Vehicle.CoordinationInformationReceive(message);
                    Become(CoordinationBehaviour);
                      
                }
            });

        }


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
