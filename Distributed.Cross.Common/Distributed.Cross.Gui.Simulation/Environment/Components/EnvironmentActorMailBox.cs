using Akka.Actor;
using Akka.Configuration;
using Akka.Dispatch;
using Distributed.Cross.Common.Communication.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distributed.Cross.Gui.Simulation.Environment.Components
{
    public class EnvironmentActorMailBox : UnboundedPriorityMailbox
    {
       

        public EnvironmentActorMailBox(Settings settings, Config config) : base(settings, config)
        { }

        protected override int PriorityGenerator(object message)
        {
            var issue = message as EnqueueNewVehicle;

            if (issue != null)
            {
                return 1;
            }

            return 0;
        }
    }
}
