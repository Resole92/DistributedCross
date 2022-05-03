using Akka.Actor;
using Distributed.Cross.Common.Actors;
using Distributed.Cross.Common.Communication.Messages;
using Distributed.Cross.Common.Module;
using Distributed.Cross.Common.Utilities;
using Distributed.Cross.Gui.Simulation.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Distributed.Cross.Gui.Simulation.Environment
{
    public class EnvironmentViewModel : INotifyPropertyChanged
    {
        private static EnvironmentViewModel _instance;
        public static EnvironmentViewModel Instance => _instance ??= new EnvironmentViewModel();

        private int _vehicle1Destination = 0;
        public int Vehicle1Destination
        {
            get => _vehicle1Destination;
            set
            {
                _vehicle1Destination = value;
                NotifyPropertyChanged();
            }
        }

        private int _vehicle2Destination = 0;
        public int Vehicle2Destination
        {
            get => _vehicle2Destination;
            set
            {
                _vehicle2Destination = value;
                NotifyPropertyChanged();
            }
        }

        private int _vehicle3Destination = 0;
        public int Vehicle3Destination
        {
            get => _vehicle3Destination;
            set
            {
                _vehicle3Destination = value;
                NotifyPropertyChanged();
            }
        }

        private int _vehicle4Destination = 0;
        public int Vehicle4Destination
        {
            get => _vehicle4Destination;
            set
            {
                _vehicle4Destination = value;
                NotifyPropertyChanged();
            }
        }

        public RelayCommand Destination1Lane5Command =>
            new RelayCommand(_ => Vehicle1Destination = 5);
        public RelayCommand Destination1Lane6Command =>
            new RelayCommand(_ => Vehicle1Destination = 6);
        public RelayCommand Destination1Lane7Command =>
            new RelayCommand(_ => Vehicle1Destination = 7);
        public RelayCommand Destination1Lane8Command =>
            new RelayCommand(_ => Vehicle1Destination = 8);

        public RelayCommand Destination2Lane5Command =>
            new RelayCommand(_ => Vehicle2Destination = 5);
        public RelayCommand Destination2Lane6Command =>
            new RelayCommand(_ => Vehicle2Destination = 6);
        public RelayCommand Destination2Lane7Command =>
            new RelayCommand(_ => Vehicle2Destination = 7);
        public RelayCommand Destination2Lane8Command =>
            new RelayCommand(_ => Vehicle2Destination = 8);

        public RelayCommand Destination3Lane5Command =>
            new RelayCommand(_ => Vehicle3Destination = 5);
        public RelayCommand Destination3Lane6Command =>
            new RelayCommand(_ => Vehicle3Destination = 6);
        public RelayCommand Destination3Lane7Command =>
            new RelayCommand(_ => Vehicle3Destination = 7);
        public RelayCommand Destination3Lane8Command =>
            new RelayCommand(_ => Vehicle3Destination = 8);

        public RelayCommand Destination4Lane5Command =>
            new RelayCommand(_ => Vehicle4Destination = 5);
        public RelayCommand Destination4Lane6Command =>
            new RelayCommand(_ => Vehicle4Destination = 6);
        public RelayCommand Destination4Lane7Command =>
            new RelayCommand(_ => Vehicle4Destination = 7);
        public RelayCommand Destination4Lane8Command =>
            new RelayCommand(_ => Vehicle4Destination = 8);


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

            });


        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
