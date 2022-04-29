using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Enums
{
    public enum RoundState
    {
        LeaderElection,
        Coordination,
        Waiting,
        Crossing,
    }
}
