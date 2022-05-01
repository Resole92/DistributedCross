using Akka.Actor;
using Distributed.Cross.Common.Communication.Messages;
using Distributed.Cross.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Module
{
    public class Vehicle : ReceiveActor
    {
        private RoundState _roundState;
        private int _leaderIdentifier;
        public int DestinationLane { get; private set; }
        public int Identifier { get; set; }
        private int _actualPosition;
        public int Priority { get; set; }
        private Logger _logger = new Logger();
        private CrossMap _map;

        public Vehicle() { }

        public Vehicle(int startLane, int destinationLane) {

            DestinationLane = destinationLane;
            Identifier = startLane;

            Context.ActorOf(Props(1, 4), "WaitingSubaru");

            Receive<ElectionStart>(message => {
                _logger.LogInformation("An election is start...");
                //Sender.Tell(message);
            });

            Receive<TestRequest>(message => {
                _logger.LogInformation($"Message come with data {message.Message}");
                Sender.Tell(new TestReponse
                {
                    Message = "Bella raga"
                }, Self); 
            });

        }

        public static Props Props(int startLane, int destinationLane)
        {
            return Akka.Actor.Props.Create(() => new Vehicle(startLane, destinationLane));
        }



    }


    public class WaitingVehicle : ReceiveActor
    {

    }

    public class CrossingVehicle : ReceiveActor
    {

    }
}
