using Distributed.Cross.Common.Module;
using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Algorithm
{
    public class TrajectoryAlgorithm
    {
        private CrossMap _map;
        private int _nodeIdentifier;

        public TrajectoryAlgorithm(CrossMap map, int nodeIdentifier)
        {
            _map = map;
            _nodeIdentifier = nodeIdentifier;
        }

        public TrajectoryResult Calculate()
        {
            throw new NotImplementedException();
        }
    }

    public class TrajectoryResult
    {
        public int Identifier { get; set; }
        public List<int> Trajectory { get; set; }
    }
}
