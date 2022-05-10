using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Communication.Messages
{
    public class BrokenVehicleRequest
    {
        public int Identifier { get; set; }

    }

    public class BrokenVehicleResponse
    {
        public List<int> BrokenNodes { get; set; }
       
    }
}
