using Akka.Actor;
using Distributed.Cross.Common.Communication.Messages;
using Distributed.Cross.Common.Module;
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
        private CrossMap _map;

        public NodeActor(int identifier, CrossMap map, Dictionary<int, IActorRef> actorsMap)
        {
            Identifier = identifier;
            ActorsMap = actorsMap;
            _map = map;

            Receive<VehicleOnNodeNotification>(message =>
            _vehicle = new Vehicle(message.Vehicle, map, this));

            Receive<VehicleRemoveNotification>(message =>
            {
                _vehicle.RemoveParentNode();
                this._vehicle = null;
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

            Receive<LeaderElectionRequest>(message =>
            {
                if(_vehicle is not null)
                {
                    Sender.Tell(new LeaderElectionResponse(), Self);
                    _logger.LogInformation($"A leader election is requested");
                    Task.Run(() =>
                    {
                        _vehicle.LeaderRequestAsk();
                    });
                    
                }
            });

            Receive<ElectionStart>(message => {
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

            Receive<TestRequest>(message => {
                if (_vehicle is not null)
                {
                    _logger.LogInformation($"Message come with data {message.Message}");
                    Sender.Tell(new TestReponse
                    {
                        Message = $"I'm {Identifier}"
                    }, Self);
                }
            });
        }

        public void ComunicatePresence()
        {
            Sender.Tell(new LeaderElectionResponse(), Self);
        }

        public static Props Props(int identifier, CrossMap map, Dictionary<int, IActorRef> actorsMap)
        {
            return Akka.Actor.Props.Create(() => new NodeActor(identifier, map, actorsMap));
        }


    }
}
