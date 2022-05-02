using Akka.Actor;
using Distributed.Cross.Common.Communication.Messages;
using Distributed.Cross.Common.Module;
using Distributed.Cross.Common.Utilities;
using System;
using System.Collections.Generic;
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

        public NodeActor(int identifier, CrossBuilder builder, Dictionary<int, IActorRef> actorsMap)
        {
            Identifier = identifier;
            ActorsMap = actorsMap;
            _builder = builder;
            StartBehaviour();
        }



        public void StartBehaviour()
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
           _vehicle = new Vehicle(message.Vehicle, _builder, this));

            Receive<VehicleRemoveNotification>(message =>
            {
                if (_vehicle is null) return;
                _vehicle.RemoveParentNode();
                _vehicle = null;
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
                    _vehicle.CoordinationInformationReceive(message);
                }
            });
        }



        public void ComunicatePresence()
        {
            Sender.Tell(new LeaderElectionResponse(), Self);
        }

        public static Props Props(int identifier, CrossBuilder builder, Dictionary<int, IActorRef> actorsMap)
        {
            return Akka.Actor.Props.Create(() => new NodeActor(identifier, builder, actorsMap));
        }


    }
}
