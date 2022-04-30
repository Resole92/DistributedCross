﻿using Distributed.Cross.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Module
{
    public class Vehicle
    {
        private RoundState _roundState;
        private int _leaderIdentifier;
        public int DestinationLane { get; set; }
        public int Identifier { get; set; }
        private int _actualPosition;
        public int Priority { get; private set; }
        private Logger _logger;
        private CrossMap _map;

        public Vehicle() { }



    }
}
