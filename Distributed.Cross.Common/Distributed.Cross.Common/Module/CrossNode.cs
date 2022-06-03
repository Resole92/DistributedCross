using Distributed.Cross.Common.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Module
{
    public class CrossNode
    {
        public CrossNodeType Type { get; set; }
        public int Identifier { get; set; }
        public VehicleDto Vehicle {get ;set; }
    }

    public enum CrossNodeType
    {
        Cross,
        Input,
        Output,
    }
}
