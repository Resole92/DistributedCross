using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Akka.Actor;
using Distributed.Cross.Common.Actors;
using Distributed.Cross.Common.Communication.Environment;
using Distributed.Cross.Common.Communication.Messages;
using Distributed.Cross.Common.Module;
using Distributed.Cross.Common.Utilities;
using Distributed.Cross.Gui.Simulation.Utilities;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Collections.ObjectModel;
using Distributed.Cross.Gui.Simulation.Environment.Components;

namespace Distributed.Cross.Gui.Simulation.Environment
{
    /// <summary>
    /// Logica di interazione per EnvironmentUC.xaml
    /// </summary>
    public partial class EnvironmentUC : UserControl
    {
        public EnvironmentUC()
        {
            InitializeComponent();
        }
    }


    public class EnvironmentViewModel : NotifyPropertyChanged
    {
        private static EnvironmentViewModel _instance;
        public static EnvironmentViewModel Instance => _instance ??= new EnvironmentViewModel();

        private ObservableCollection<LaneQueue> _queues = new ObservableCollection<LaneQueue>();
        public ObservableCollection<LaneQueue> Queues
        {
            get => _queues;
            set
            {
                _queues = value;
                OnPropertyChanged();
            }
        }

        public void AddNewLaneItem(int lane, QueueItem item)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {

                var targetQueue = _queues.FirstOrDefault(x => x.LaneNumber == lane);
                if (targetQueue is null)
                {
                    targetQueue = new LaneQueue
                    {
                        LaneNumber = lane
                    };
                    Queues.Add(targetQueue);
                    Queues = new ObservableCollection<LaneQueue>(Queues.OrderBy(x => x.LaneNumber));
                }

                targetQueue.Queue.Add(item);
            });

        }

        public void RemoveLaneItem(QueueItem item)
            => Application.Current.Dispatcher.Invoke(() => _queues.ToList().ForEach(x => x.Queue.Remove(item)));



        EnvironmentViewModel()
        {
            EnvironmentInitialization();
        }

        private int _vehicle1Destination = 0;
        public int Vehicle1Destination
        {
            get => _vehicle1Destination;
            set
            {
                _vehicle1Destination = value;
                OnPropertyChanged();
            }
        }

        private int _vehicle2Destination = 0;
        public int Vehicle2Destination
        {
            get => _vehicle2Destination;
            set
            {
                _vehicle2Destination = value;
                OnPropertyChanged();
            }
        }

        private int _vehicle3Destination = 0;
        public int Vehicle3Destination
        {
            get => _vehicle3Destination;
            set
            {
                _vehicle3Destination = value;
                OnPropertyChanged();
            }
        }

        private int _vehicle4Destination = 0;
        public int Vehicle4Destination
        {
            get => _vehicle4Destination;
            set
            {
                _vehicle4Destination = value;
                OnPropertyChanged();
            }
        }

        private int _actualRound = 0;
        public int ActualRound
        {
            get => _actualRound;
            set
            {
                _actualRound = value;
                OnPropertyChanged();
            }
        }



        private int _selectedVehicle = 8;
        public int SelectedVehicle
        {
            get => _selectedVehicle;
            set
            {
                _selectedVehicle = value;
                OnPropertyChanged();
            }
        }

        private int _numberRandomVehicle = 1;
        public int NumberRandomVehicle
        {
            get => _numberRandomVehicle;
            set
            {
                _numberRandomVehicle = value;
                OnPropertyChanged();
            }
        }


        private void EnvironmentInitialization()
        {
            ActorSystem system = ActorSystem.Create("MySystem");

            var map = AlgorithmSimulation.AlgorithmViewModel.Instance.BuildEmptyMap();
            var totalNode = map.Map.GetAllNodes().Count();

            var environmentActor = system.ActorOf(EnvironmentActor.Props(_actors), "Environment");
            _actors.Add(-1, environmentActor);

            for (var actorname = 0; actorname < map.Map.GetAllNodes().Count(); actorname++)
            {
                var actor = system.ActorOf(NodeActor.Props(actorname, new CrossBuilder(3, 3), _actors), actorname.ToString());
                _actors.Add(actorname, actor);

            }
        }

        public RelayCommand RequestInformationVehicle =>
            new RelayCommand(_ =>
            {
                var actor = _actors[SelectedVehicle];
                var response = actor.Ask<InformationVehicleResponse>(new InformationVehicleRequest(), TimeSpan.FromSeconds(5));

                var result = response.Result;
            });

        public RelayCommand Destination1Lane5Command =>
            new RelayCommand(_ =>
            {
                Vehicle1Destination = 0;
                Vehicle1Destination = 5;
            });
        public RelayCommand Destination1Lane6Command =>
            new RelayCommand(_ =>
            {
                Vehicle1Destination = 0;
                Vehicle1Destination = 6;
            });
        public RelayCommand Destination1Lane7Command =>
             new RelayCommand(_ =>
             {
                 Vehicle1Destination = 0;
                 Vehicle1Destination = 7;
             });
        public RelayCommand Destination1Lane8Command =>
            new RelayCommand(_ =>
            {
                Vehicle1Destination = 0;
                Vehicle1Destination = 8;
            });

        public RelayCommand Destination2Lane5Command =>
            new RelayCommand(_ =>
            {
                Vehicle2Destination = 0;
                Vehicle2Destination = 5;
            });
        public RelayCommand Destination2Lane6Command =>
            new RelayCommand(_ =>
            {
                Vehicle2Destination = 0;
                Vehicle2Destination = 6;
            });
        public RelayCommand Destination2Lane7Command =>
            new RelayCommand(_ =>
            {
                Vehicle2Destination = 0;
                Vehicle2Destination = 7;
            });
        public RelayCommand Destination2Lane8Command =>
            new RelayCommand(_ =>
            {
                Vehicle2Destination = 0;
                Vehicle2Destination = 8;
            });

        public RelayCommand Destination3Lane5Command =>
            new RelayCommand(_ =>
            {
                Vehicle3Destination = 0;
                Vehicle3Destination = 5;
            });
        public RelayCommand Destination3Lane6Command =>
            new RelayCommand(_ =>
            {
                Vehicle3Destination = 0;
                Vehicle3Destination = 6;
            });
        public RelayCommand Destination3Lane7Command =>
            new RelayCommand(_ =>
            {
                Vehicle3Destination = 0;
                Vehicle3Destination = 7;
            });
        public RelayCommand Destination3Lane8Command =>
            new RelayCommand(_ =>
            {
                Vehicle3Destination = 0;
                Vehicle3Destination = 8;
            });

        public RelayCommand Destination4Lane5Command =>
            new RelayCommand(_ =>
            {
                Vehicle4Destination = 0;
                Vehicle4Destination = 5;
            }
           );
        public RelayCommand Destination4Lane6Command =>
            new RelayCommand(_ =>
            {
                Vehicle4Destination = 0;
                Vehicle4Destination = 6;
            });
        public RelayCommand Destination4Lane7Command =>
            new RelayCommand(_ =>
            {
                Vehicle4Destination = 0;
                Vehicle4Destination = 7;
            });
        public RelayCommand Destination4Lane8Command =>
            new RelayCommand(_ =>
            {
                Vehicle4Destination = 0;
                Vehicle4Destination = 8;
            });

        private Dictionary<int, IActorRef> _actors = new Dictionary<int, IActorRef>();


        private int _selectedExample;
        public int SelectedExample
        {
            get => _selectedExample;
            set
            {
                _selectedExample = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand AddRandomVehicleCommand =>
            new RelayCommand(_ =>
            {
                Task.Run(() =>
                {

                    var environemt = _actors[-1];
                    Random randInput = new Random(Guid.NewGuid().GetHashCode());

                    for (var i = 0; i < _numberRandomVehicle; i++)
                    {

                        var entryLane = randInput.Next(1, 5);
                        var exitLane = randInput.Next(5, 9);

                        environemt.Tell(new EnqueueNewVehicle
                        {
                            DestinationLane = exitLane,
                            StartLane = entryLane
                        });
                    }
                });
            });

        public RelayCommand StartEnvironmentCommand
            => new RelayCommand(_ =>
            {

                Task.Run(() =>
                {



                    //if(SelectedExample == 0)
                    //Example0(_actors);
                    //else
                    Example0(_actors);
                });

            });

        /// <summary>
        /// In this example there are 3 vehicle and 3 round
        /// </summary>
        /// <param name="actors"></param>
        public void Example0(Dictionary<int, IActorRef> actors)
        {
            var actor1 = actors[1];
            var actor2 = actors[2];
            var actor3 = actors[3];

            actor1.Tell(new VehicleOnNodeRequest
            {
                Vehicle = new Common.Data.VehicleDto
                {
                    StartLane = 1,
                    DestinationLane = 6,
                }
            });


            actor2.Tell(new VehicleOnNodeRequest
            {
                Vehicle = new Common.Data.VehicleDto
                {
                    StartLane = 2,
                    DestinationLane = 7,
                }
            });

            actor3.Tell(new VehicleOnNodeRequest
            {
                Vehicle = new Common.Data.VehicleDto
                {
                    StartLane = 3,
                    DestinationLane = 5,
                }
            });

        }

        /// <summary>
        /// For vehicle and 1 round
        /// </summary>
        public void Example1(Dictionary<int, IActorRef> actors)
        {
            var actor1 = actors[1];
            var actor2 = actors[2];
            var actor3 = actors[3];
            var actor4 = actors[4];

            actor1.Tell(new VehicleOnNodeRequest
            {
                Vehicle = new Common.Data.VehicleDto
                {
                    StartLane = 1,
                    DestinationLane = 8,
                }
            });

            Thread.Sleep(500);

            actor2.Tell(new VehicleOnNodeRequest
            {
                Vehicle = new Common.Data.VehicleDto
                {
                    StartLane = 2,
                    DestinationLane = 5,
                }
            });

            Thread.Sleep(500);

            actor3.Tell(new VehicleOnNodeRequest
            {
                Vehicle = new Common.Data.VehicleDto
                {
                    StartLane = 3,
                    DestinationLane = 6,
                }
            });

            Thread.Sleep(3000);

            actor4.Tell(new VehicleOnNodeRequest
            {
                Vehicle = new Common.Data.VehicleDto
                {
                    StartLane = 4,
                    DestinationLane = 7,
                }
            });
        }

    }
}
