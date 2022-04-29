using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Module
{
    public class CrossMap
    {
        public List<CrossNode> InputLane { get; set; }
        public List<CrossNode> OuputLane { get; set; }
        public Graph<CrossNode> Map { get; set; }

        public CrossMap() { }

    }
}
