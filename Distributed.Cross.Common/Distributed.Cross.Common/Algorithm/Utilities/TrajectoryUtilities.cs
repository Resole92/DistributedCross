using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Distributed.Cross.Common.Algorithm.Utilities
{


    public class DijkstraResult
    {
        public double Distance { get; set; }
        public int ParentIdentifier { get; set; }
    }


    public class TrajectoryResult
    {
        public int Identifier { get; set; }
        public List<int> Trajectory { get; set; } = new List<int>();

        public bool IsTrajectoryFound => Trajectory.Any();

 
    }
}
