using Distributed.Cross.Common.Data;
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
        public int InputLane
        {
            get => _startLane;
            set
            {
                _startLane = value;
                OnPropertyChanged();
            }
        }

        private int _endLane;
        public int OutputLane
        {
            get => _endLane;
            set
            {
                _endLane = value;
                OnPropertyChanged();
            }
        }

        private int _priority = 1;
        public int Priority
        {
            get => _priority;
            set
            {
                _priority = value;
                OnPropertyChanged();
            }
        }

        private double _speed;
        public double Speed
        {
            get => _speed;
            set
            {
                _speed = value;
                OnPropertyChanged();
            }
        }

        private int? _brokenNode;
        public int? BrokenNode
        {
            get => _brokenNode;
            set
            {
                _brokenNode = value;
                OnPropertyChanged();
            }
        }

        private int _licensePlate;
        public int LicensePlate
        {
            get => _licensePlate;
            set
            {
                _licensePlate = value;
                OnPropertyChanged();
            }
        }

        private bool _isGhostOnEndRound;
        public bool IsGhostOnEndRound
        {
            get => _isGhostOnEndRound;
            set
            {
                _isGhostOnEndRound = value;
                OnPropertyChanged();
            }
        }

        private bool _isGhostOnCoordination;
        public bool IsGhostOnCoordination
        {
            get => _isGhostOnCoordination;
            set
            {
                _isGhostOnCoordination = value;
                OnPropertyChanged();
            }
        }

        public VehicleGui(VehicleDto vehicle)
        {
            InputLane = vehicle.InputLane;
            OutputLane = vehicle.OutputLane;
            Speed = vehicle.Speed;
            BrokenNode = vehicle.BrokenNode;
            IsGhostOnEndRound = vehicle.IsGhostOnEndRound;
            IsGhostOnCoordination = vehicle.IsGhostOnCoordination;
            LicensePlate = vehicle.LicensePlate;
        }
    }
}
