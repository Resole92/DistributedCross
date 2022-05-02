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
        private Vehicle _vehicle;
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

            EntryBehaviour();
        }

        private void BuildMap(CrossBuilder builder)
        {
            builder.CreateBasicInputOutput();
            var crossMap = builder.Build();
            _map = crossMap;
        }


        public void EntryBehaviour()
        {
           

            Receive<LeaderElectionRequest>(message =>
            {
                if (_vehicle is not null)
                {
                    Sender.Tell(new LeaderElectionResponse(), Self);
                    _logger.LogInformation($"A leader election is requested");

                    var self = Self;
                    Task.Run(() =>
                    {
                        _vehicle.LeaderRequestAsk();
                    });//.PipeTo(self);

                    //ReceiveAny(o => Stash.Stash());

                    Become(ElectionBehaviour);
                }
            });

            Receive<ElectionStart>(message =>
            {
                if (_vehicle is not null)
                {
                    _logger.LogInformation("An election is start...");
                    Task.Run(() =>
                    {
                        _vehicle.LeaderRequestAsk();
                    });

                    Become(ElectionBehaviour);
                }
                //Sender.Tell(message);
            });


            BaseBehaviour();
        }


        public void ElectionBehaviour()
        {

            Receive<ElectionStart>(message =>
            {
                if (_vehicle is not null)
                {
                    _logger.LogInformation("An election is already started...");
                }
            });

            Receive<LeaderElectionRequest>(message =>
            {
                if (_vehicle is not null)
                {
                    Sender.Tell(new LeaderElectionResponse(), Self);
                    _logger.LogInformation($"A leader election is requested");
                }
            });

            BaseBehaviour();
        }

        private void BaseBehaviour()
        {
            Receive<VehicleOnNodeNotification>(message =>
            {
                _vehicle = new Vehicle(message.Vehicle, _builder, this);
                Self.Tell(new ElectionStart());
            });

            Receive<VehicleRemoveNotification>(message =>
            {
                if (_vehicle is null) return;
                _vehicle.RemoveParentNode();
                _vehicle = null;

                Become(EntryBehaviour);
               
            });

            Receive<LeaderNotificationRequest>(message =>
            {
                if (_vehicle is not null)
                {
                    _logger.LogInformation($"A leader is elected with id {message.Identifier}");
                    var data = _vehicle.LeaderElected(message);
                    Sender.Tell(new LeaderNotificationResponse
                    {
                        VehicleDetail = data
                    }, Self);
                }
            });

            Receive<CoordinationNotificationRequest>(message =>
            {
                if (_vehicle is not null)
                {
                    _logger.LogInformation($"Coordination data received!");
                    var isRunner = _vehicle.CoordinationInformationReceive(message);
                    Become(EntryBehaviour);
                      
                }
            });

            Receive<VehicleExitNotification>(message =>
            {
                if (_vehicle is not null)
                {
                    _vehicle.VehicleExit(message.Identifier);
                }
            });

            Receive<VehicleMoveNotification>(message =>
            {
                _vehicle = new Vehicle(message.Vehicle, _builder, this);
                _vehicle.UpdateCrossingStatus(message);
                _vehicle.StartCrossing();
            });
        }


        public static Props Props(int identifier, CrossBuilder builder, Dictionary<int, IActorRef> actorsMap)
        {
            return Akka.Actor.Props.Create(() => new NodeActor(identifier, builder, actorsMap));
        }


    }
}
