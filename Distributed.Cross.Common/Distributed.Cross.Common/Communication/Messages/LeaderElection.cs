using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Communication.Messages
{
    public class LeaderElectionRequest
    {
        public int Identifier { get; set; }
    }

    public class LeaderElectionResponse
    {
        public bool Acknowledge { get; set; } = true;
    }
}
