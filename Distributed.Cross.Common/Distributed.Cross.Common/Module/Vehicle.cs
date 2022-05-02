using Akka.Actor;
using Distributed.Cross.Common.Communication.Messages;
using Distributed.Cross.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Module
{
    public class Vehicle
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


        }
    }
}
