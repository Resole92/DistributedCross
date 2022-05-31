using Distributed.Cross.Common.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Communication.Messages
{


    public class ElectionResult
    {
        public ElectionResultType Result { get; set; }
        public ElectionResult(ElectionResultType result)
        {
            Result = result;
        }
    }

    public enum ElectionResultType
    {
        
        Cancelled,
        Crossing,
        Bully,
        LeaderAlreadyPresent,
        Elected,
        NotHandled
    }
}
