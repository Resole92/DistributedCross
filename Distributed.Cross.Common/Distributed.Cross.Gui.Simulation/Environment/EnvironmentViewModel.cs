using Akka.Actor;
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


        public RelayCommand StartEnvironmentCommand
            => new RelayCommand(_ =>
            {
                Task.Run(() =>
                {
                    
                });

                ActorSystem system = ActorSystem.Create("MySystem");
                system.ActorOf(Vehicle.Props(1, 4), "Subaru");
                system.ActorOf(Vehicle.Props(2, 5), "Fiat");

                var actors = system.ActorSelection($"akka://MySystem/user");
                //var waitingSubaru = system.ActorSelection("akka://MySystem/user/Subaru/WaitingSubaru");


                //var response = waitingSubaru.Ask<TestReponse>(new TestRequest
                //{
                //    Message = "cacchio"
                //});

                //Console.WriteLine($"Response received: {response.Result.Message}");


            });
    }
}
