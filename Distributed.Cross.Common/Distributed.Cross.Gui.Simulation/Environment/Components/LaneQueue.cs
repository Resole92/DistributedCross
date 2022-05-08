using Distributed.Cross.Gui.Simulation.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Distributed.Cross.Gui.Simulation.Environment.Components
{
    public class LaneQueue : NotifyPropertyChanged
    {

        private int _laneNumber;
        public int LaneNumber
        {
            get => _laneNumber;
            set
            {
                _laneNumber = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<QueueItem> _queue = new ObservableCollection<QueueItem>();
        public ObservableCollection<QueueItem> Queue
        {
            get => _queue;
            set
            {
                _queue = value;
                OnPropertyChanged();
            }
        }

        
    }


    public class QueueItem : NotifyPropertyChanged
    {

        private int _endLane;
        public int EndLane
        {
            get => _endLane;
            set
            {
                _endLane = value;
                OnPropertyChanged();
            }
        }
    }
}
