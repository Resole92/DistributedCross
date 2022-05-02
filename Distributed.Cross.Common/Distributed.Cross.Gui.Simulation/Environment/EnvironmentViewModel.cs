using Akka.Actor;
using Distributed.Cross.Common.Actors;
using Distributed.Cross.Common.Communication.Messages;
using Distributed.Cross.Common.Module;
using Distributed.Cross.Gui.Simulation.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distributed.Cross.Gui.Simulation.Environment
{
    public class EnvironmentViewModel
    {
        private static EnvironmentViewModel _instance;
        public static EnvironmentViewModel Instance => _instance ??= new EnvironmentViewModel();


        private int _numberTest;

        public RelayCommand StartEnvironmentCommand
            => new RelayCommand(_ =>
            {
                var actors = new Dictionary<int, IActorRef>();
                ActorSystem system = ActorSystem.Create("MySystem");                

                var map = AlgorithmSimulation.AlgorithmViewModel.Instance.BuildMap();
                var totalNode = map.Map.GetAllNodes().Count();

                for (var actorname = 0; actorname < map.Map.GetAllNodes().Count(); actorname++)
                {
                    var actor = system.ActorOf(NodeActor.Props(actorname, map, actors), actorname.ToString());
                    actors.Add(actorname, actor);

                }

                //var waitingSubaru = system.\($"akka://MySystem/user/*/1");
                //var waitingFiat = system.ActorSelection($"akka://MySystem/user/*/2");
                //var waitingSubaru = system.ActorSelection("akka://MySystem/user/Subaru/WaitingSubaru");


                //waitingSubaru.Tell(new TestRequest
                //{
                //    Message = "cacchio"
                //});



                _numberTest++;

                //subarissima.Tell(new TestRequest
                //{
                //    Message = $"Is round {_numberTest}"
                //});



                //var randActor = new Random((int) DateTime.Now.Ticks & 0x0000FFFF);
                //var actorNumber = randActor.Next(0, totalNode);

                //var actor1 = actors[actorNumber];

                var actor1 = actors[1];
                var actor2 = actors[2];
                var actor3 = actors[3];
                var actor4 = actors[4];

                actor1.Tell(new VehicleOnNodeNotification
                {
                    Vehicle = new Common.Data.VehicleDto
                    {
                        StartLane = 1,
                        DestinationLane = 4,
                    }
                });

                //actor2.Tell(new VehicleOnNodeNotification
                //{
                //    Vehicle = new Common.Data.VehicleDto
                //    {
                //        StartLane = 1,
                //        DestinationLane = 4,
                //    }
                //});

                actor3.Tell(new VehicleOnNodeNotification
                {
                    Vehicle = new Common.Data.VehicleDto
                    {
                        StartLane = 3,
                        DestinationLane = 4,
                    }
                });

                actor1.Tell(new ElectionStart());

                actor4.Tell(new VehicleOnNodeNotification
                {
                    Vehicle = new Common.Data.VehicleDto
                    {
                        StartLane = 4,
                        DestinationLane = 4,
                    }
                });

                actor4.Tell(new VehicleRemoveNotification());

               



                //var response = waitingSubaru.Ask<TestReponse>(new TestRequest
                //{
                //    Message =  $"Is round {_numberTest}"
                //}, TimeSpan.FromSeconds(5));



                // Console.WriteLine($"Actual test {_numberTest}");



            });
    }
}
