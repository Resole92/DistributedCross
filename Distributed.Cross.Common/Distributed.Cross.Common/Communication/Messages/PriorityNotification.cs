using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Communication.Messages
{
    public class PriorityNotification
    {
        public int Identifier { get; set; }
        public int Priority { get; set; }

        public PriorityNotification(int identifier, int priority)
        {
            Identifier = identifier;
            Priority = priority;
        }

    }
}
