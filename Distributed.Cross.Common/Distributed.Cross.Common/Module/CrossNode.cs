using Distributed.Cross.Common.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Module
{
    public class CrossNode
    {
        public CrossNodePosition Position { get; set; }
        public CrossNodeType Type { get; set; }
        public int Identifier { get; set; }
        public VehicleDto Vehicle {get ;set; }
    }

    public enum CrossNodePosition
    {
        TopLeft,
        Top,
        TopRight,
        Left,
        Internal,
        Right,
        BottomLeft,
        Bottom,
        BottomRight,
    }

    public enum CrossNodeType
    {
        Cross,
        Input,
        Output,
    }
}
