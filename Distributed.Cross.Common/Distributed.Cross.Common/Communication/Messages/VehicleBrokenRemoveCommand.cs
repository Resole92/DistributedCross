using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Communication.Messages
{
    public class VehicleBrokenRemoveCommand
    {
        public int Identifier { get; set; }

        public VehicleBrokenRemoveCommand(int identifier)
        {
            Identifier = identifier;
        }
    }
}
