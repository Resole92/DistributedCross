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


        private void CreateBasicInputOutput()
        {
            AddInputLane(0, 0);
            AddOutputLane(0, 0);
            AddInputLane(0, 3);
            AddOutputLane(0, 3);
            AddInputLane(3, 0);
            AddOutputLane(3, 0);
            AddInputLane(3, 3);
            AddOutputLane(3, 3);
        }

        public void AddInputLane(int row, int column)
            => Inputs.Add((row, column));

        public void AddOutputLane(int row, int column)
            => Outputs.Add((row, column));

        public CrossMap Build()
        {
            var map = new CrossMap(_height, _length);
            var crossMatrix = map.BuildInternalMap();
            Inputs.ForEach(x => map.AddInputLane(x.Item1, x.Item2, crossMatrix));
            Outputs.ForEach(x => map.AddOutputLane(x.Item1, x.Item2, crossMatrix));
            map.BuildIdentifier();

            return map;
        }

    }
}
