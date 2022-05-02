using Akka.Actor;
using Distributed.Cross.Common.Actors;
using Distributed.Cross.Common.Communication.Messages;
using Distributed.Cross.Common.Module;
using Distributed.Cross.Common.Utilities;
using Distributed.Cross.Gui.Simulation.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Distributed.Cross.Gui.Simulation.Environment
{
    public class EnvironmentViewModel
    {
        private static EnvironmentViewModel _instance;
        public static EnvironmentViewModel Instance => _instance ??= new EnvironmentViewModel();

        public RelayCommand StartEnvironmentCommand
            => new RelayCommand(_ =>
            {
                var actors = new Dictionary<int, IActorRef>();
                ActorSystem system = ActorSystem.Create("MySystem");                

                var map = AlgorithmSimulation.AlgorithmViewModel.Instance.BuildEmptyMap();
                var totalNode = map.Map.GetAllNodes().Count();

                for (var actorname = 0; actorname < map.Map.GetAllNodes().Count(); actorname++)
                {
                    var actor = system.ActorOf(NodeActor.Props(actorname, new CrossBuilder(3,3), actors), actorname.ToString());
                    actors.Add(actorname, actor);

                }


                var actor1 = actors[1];
                var actor2 = actors[2];
                var actor3 = actors[3];
                var actor4 = actors[4];

                actor1.Tell(new VehicleOnNodeNotification
                {
                    Vehicle = new Common.Data.VehicleDto
                    {
                        StartLane = 1,
                        DestinationLane = 6,
                    }
                });

                actor2.Tell(new VehicleOnNodeNotification
                {
                    Vehicle = new Common.Data.VehicleDto
                    {
                        StartLane = 2,
                        DestinationLane = 7,
                    }
                });


                actor3.Tell(new VehicleOnNodeNotification
                {
                    Vehicle = new Common.Data.VehicleDto
                    {
                        StartLane = 3,
                        DestinationLane = 5,
                    }
                });

                //actor1.Tell(new ElectionStart());
                //actor1.Tell(new ElectionStart());


            });
    }
}
