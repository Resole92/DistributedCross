using Distributed.Cross.Gui.Simulation.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Distributed.Cross.Gui.Simulation.Environment.Components
{
    public class VehicleGui : NotifyPropertyChanged
    {
        private int _startLane;
        public int StartLane
        {
            get => _startLane;
            set
            {
                _startLane = value;
                OnPropertyChanged();
            }
        }

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

        private int _priority;
        public int Priority
        {
            get => _priority;
            set
            {
                _priority = value;
                OnPropertyChanged();
            }
        }
    }
}
