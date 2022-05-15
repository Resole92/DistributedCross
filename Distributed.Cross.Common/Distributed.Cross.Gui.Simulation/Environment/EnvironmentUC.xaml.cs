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

        public string PathVehicle1 => "M 70,40 V 50 Q 100,120 120,50 V 10";

        private ObservableCollection<VehicleGui> _outputVehicles = new();
        public ObservableCollection<VehicleGui> OutputVehicles
        {
            get => _outputVehicles;
            set
            {
                _outputVehicles = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<VehicleGui> _inputVehicles = new();
        public ObservableCollection<VehicleGui> InputVehicles
        {
            get => _inputVehicles;
            set
            {
                _inputVehicles = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<VehicleGui> _crossVehicles = new();
        public ObservableCollection<VehicleGui> CrossVehicles
        {
            get => _crossVehicles;
            set
            {
                _crossVehicles = value;
                OnPropertyChanged();
            }
        }

        public void AddNewLaneItem(int lane, VehicleGui item)
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

        public void RemoveLaneItem(VehicleGui item)
            => Application.Current.Dispatcher.Invoke(() => _queues.ToList().ForEach(x => x.Queue.Remove(item)));


        private bool _isTechModeEnable = false;
        public bool IsTechModeEnable
        {
            get => _isTechModeEnable;
            set
            {
                _isTechModeEnable = value;
                OnPropertyChanged();
            }
        }

        private bool _isNormalModeEnable = true;
        public bool IsNormalModeEnable
        {
            get => _isNormalModeEnable;
            set
            {
                _isNormalModeEnable = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand EnableTechModeCommand =>
            new RelayCommand(_ => IsTechModeEnable = !IsTechModeEnable);
        public RelayCommand EnableNormalModeCommand =>
            new RelayCommand(_ => IsNormalModeEnable = !IsNormalModeEnable);


        EnvironmentViewModel()
        {
            EnvironmentInitialization();
            for(var i = 0; i < 4; i++)
            {
                InputVehicles.Add(null);
                OutputVehicles.Add(null);
            }

            for(var i = 0; i < 9; i++)
            {
                CrossVehicles.Add(null);
            }

            for(var i = 0; i <17; i++)
            {
                TechNodes.Add(0);
            }
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

        private int _leaderIdentifier;
        public int LeaderIdentifier
        {
            get => _leaderIdentifier;
            set
            {
                _leaderIdentifier = value;
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

        private int _numberRandomVehicle = 20;
        public int NumberRandomVehicle
        {
            get => _numberRandomVehicle;
            set
            {
                _numberRandomVehicle = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<int> _techNodes = new ();
        public ObservableCollection<int> TechNodes
        {
            get => _techNodes;
            set
            {
                _techNodes = value;
                OnPropertyChanged();
            }
        }


        private void EnvironmentInitialization()
        {
            ActorSystem system = ActorSystem.Create("MySystem");

            var map = AlgorithmSimulation.AlgorithmViewModel.Instance.BuildEmptyMap();
            var totalNode = map.Map.GetAllNodes().Count();

            var environmentActor = system.ActorOf(EnvironmentActor.Props(_actors), "Environment");
            _actors.Add(Const.EnvironmentIdentifier, environmentActor);

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

        private int _selectedBrokenVehicle = 9;
        public int SelectedBrokenVehicle
        {
            get => _selectedBrokenVehicle;
            set
            {
                _selectedBrokenVehicle = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand AddBrokenVehicleCommand =>
            new RelayCommand(_ =>
            {
                var environemt = _actors[Const.EnvironmentIdentifier];
                environemt.Tell(new EnqueueNewVehicle(new Common.Data.VehicleDto
                {
                    BrokenNode = SelectedBrokenVehicle,
                    OutputLane = 8,
                    InputLane = 1,
                    Speed = 3.5,
                }));
            });

        private int _selectedRemoveBrokenVehicle = 9;
        public int SelectedRemoveBrokenVehicle
        {
            get => _selectedRemoveBrokenVehicle;
            set
            {
                _selectedRemoveBrokenVehicle = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand RemoveBrokenVehicleCommand =>
            new RelayCommand(_ =>
            {
                var brokenNode = _actors[Const.BrokenIdentifier];
                brokenNode.Tell(new VehicleBrokenRemoveCommand(SelectedRemoveBrokenVehicle));
            });

        public RelayCommand AddRandomVehicleCommand =>
            new RelayCommand(_ =>
            {
                Task.Run(() =>
                {

                    var environemt = _actors[Const.EnvironmentIdentifier];
                    Random randInput = new Random(Guid.NewGuid().GetHashCode());

                    for (var i = 0; i < _numberRandomVehicle; i++)
                    {

                        var entryLane = randInput.Next(1, 5);
                        var exitLane = randInput.Next(5, 9);

                        environemt.Tell(new EnqueueNewVehicle(new Common.Data.VehicleDto
                        {
                            OutputLane = exitLane,
                            InputLane = entryLane,
                            Speed = 3.5,
                        }));
                    }
                });
            });

        public RelayCommand StartEnvironmentCommand
            => new RelayCommand(_ =>
            {

                Task.Run(() =>
                {

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

            actor1.Tell(new VehicleOnNodeCommand
            {
                Vehicle = new Common.Data.VehicleDto
                {
                    InputLane = 1,
                    OutputLane = 6,
                }
            });


            actor2.Tell(new VehicleOnNodeCommand
            {
                Vehicle = new Common.Data.VehicleDto
                {
                    InputLane = 2,
                    OutputLane = 7,
                }
            });

            actor3.Tell(new VehicleOnNodeCommand
            {
                Vehicle = new Common.Data.VehicleDto
                {
                    InputLane = 3,
                    OutputLane = 5,
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

            actor1.Tell(new VehicleOnNodeCommand
            {
                Vehicle = new Common.Data.VehicleDto
                {
                    InputLane = 1,
                    OutputLane = 8,
                }
            });

            Thread.Sleep(500);

            actor2.Tell(new VehicleOnNodeCommand
            {
                Vehicle = new Common.Data.VehicleDto
                {
                    InputLane = 2,
                    OutputLane = 5,
                }
            });

            Thread.Sleep(500);

            actor3.Tell(new VehicleOnNodeCommand
            {
                Vehicle = new Common.Data.VehicleDto
                {
                    InputLane = 3,
                    OutputLane = 6,
                }
            });

            Thread.Sleep(3000);

            actor4.Tell(new VehicleOnNodeCommand
            {
                Vehicle = new Common.Data.VehicleDto
                {
                    InputLane = 4,
                    OutputLane = 7,
                }
            });
        }

    }
}
