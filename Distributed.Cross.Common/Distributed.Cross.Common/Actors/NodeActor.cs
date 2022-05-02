using Akka.Actor;
using Distributed.Cross.Common.Communication.Messages;
using Distributed.Cross.Common.Module;
using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Actors
{
    public class NodeActor : ReceiveActor
    {
        private int _identifier;
        private Vehicle _vehicle;
        private Logger _logger = new Logger();
        public NodeActor(int identifier)
        {
            _identifier = identifier;

            Receive<ElectionStart>(message => {
                _logger.LogInformation("An election is start...");
                //Sender.Tell(message);
            });

            Receive<TestRequest>(message => {
                _logger.LogInformation($"Message come with data {message.Message}");
                Sender.Tell(new TestReponse
                {
                    Message = $"I'm {_identifier}"
                }, Self);
            });
        }

        public static Props Props(int identifier)
        {
            return Akka.Actor.Props.Create(() => new NodeActor(identifier));
        }


    }
}
