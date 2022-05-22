using Distributed.Cross.Common.Module;
using System;
using System.Collections.Generic;
using System.Text;

namespace Distributed.Cross.Common.Utilities
{
    public class CrossBuilder
    {
        public List<(int, int)> Inputs = new List<(int, int)>();
        public List<(int, int)> Outputs = new List<(int, int)>();

        private int _height;
        private int _length;
        public CrossBuilder(int height, int lenght)
        {
            _height = height;
            _length = lenght;
        }


        public CrossBuilder CreateBasicInputOutput()
        {
            AddInputLane(0, 0);
            AddInputLane(0, _length - 1);
            AddInputLane(_height - 1, _length - 1);
            AddInputLane(_height - 1, 0);

            AddOutputLane(0, _length - 1);
            AddOutputLane(_height - 1, _length - 1);
            AddOutputLane(_height - 1, 0);
            AddOutputLane(0, 0);

            return this;
        }

        public void AddInputLane(int row, int column)
            => Inputs.Add((row, column));

        public void AddOutputLane(int row, int column)
            => Outputs.Add((row, column));

        public CrossMap Build()
        {
            var map = new CrossMap(_height, _length);
            map.BuildInternalMap();
            Inputs.ForEach(x => map.AddInputLane(x.Item1, x.Item2));
            Outputs.ForEach(x => map.AddOutputLane(x.Item1, x.Item2));
            map.BuildIdentifier();

            Inputs.Clear();
            Outputs.Clear();

            return map;
        }

    }
}
