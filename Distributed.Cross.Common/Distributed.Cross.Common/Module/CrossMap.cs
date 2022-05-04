using Distributed.Cross.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Distributed.Cross.Common.Module
{
    public class CrossMap
    {

        public Graph<CrossNode> Map { get; set; } = new Graph<CrossNode>();
        private CrossNode[,] _crossGrid;

        public int Height { get; private set; }
        public int Lenght { get; private set; }

        public CrossMap(int height, int lenght)
        {
            Height = height;
            Lenght = lenght;
        }

        public void EraseMapFromVehicles()
        {
            foreach (var node in Map.GetAllNodes())
            {
                node.Vehicle = null;
            }
        }


        public CrossNode[,] BuildInternalMap()
        {

            var crossMatrix = new CrossNode[Height, Lenght];

            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Lenght; column++)
                {
                    crossMatrix[row,column] = new CrossNode
                    {
                        Type = CrossNodeType.Cross,
                    };
                }
            }


            for (int row = 0; row < Height; row ++)
            {
                for (int column = 0; column < Lenght; column++)
                {
                    var node = crossMatrix[row, column];

                    if (row != Height -1)
                    {
                        var bottom = crossMatrix[row + 1, column];
                        Map.AddNeighbor(node, bottom, 1);

                        if (column != 0)
                        {
                            var bottomLeft = crossMatrix[row + 1, column -1];
                            Map.AddNeighbor(node, bottomLeft, 1.4);
                        }

                        if (column != Lenght - 1)
                        {
                            var bottomRight = crossMatrix[row + 1, column + 1];
                            Map.AddNeighbor(node, bottomRight, 1.4);
                        }
                    }

                    if(column != Lenght -1 )
                    {
                        var left = crossMatrix[row, column + 1];
                        Map.AddNeighbor(node, left, 1);
                    }
                }
            }
            _crossGrid = crossMatrix;
            return crossMatrix;
        }


        public void BuildIdentifier()
        {
            var count = 1;
            foreach(var input in Map.GetAllNodes().Where(x => x.Type == CrossNodeType.Input))
            {
                input.Identifier = count++;
            }
            
            foreach(var output in Map.GetAllNodes().Where(x => x.Type == CrossNodeType.Output))
            {
                output.Identifier = count++;
            }

            for (int row = 0; row < Height; row++)
                for (int column = 0; column < Lenght; column++)
                    _crossGrid[row, column].Identifier = count++;

        }


        /// <summary>
        /// Valid only if node choose exists and is on perimeter
        /// </summary>
        /// <param name="externalNodeId"></param>
        public void AddInputLane(int rowCell, int columnCell)
        {

            var inputNode = new CrossNode
            {
                Type = CrossNodeType.Input
            };

            var targetNode = _crossGrid[rowCell, columnCell];
            Map.AddDirectNeighbor(inputNode, targetNode,1);

        }

        /// <summary>
        /// Valid only if node choose exists and is on perimeter
        /// </summary>
        /// <param name="externalNodeId"></param>
        public void AddOutputLane(int rowCell, int columnCell)
        {
            var outputNode = new CrossNode
            {
                Type = CrossNodeType.Output
            };

            var targetNode = _crossGrid[rowCell, columnCell];
            Map.AddDirectNeighbor(targetNode, outputNode, 1);
        }


        public void AddVehicle(VehicleDto vehicle)
        {
            var node = Map.GetAllNodes().FirstOrDefault(x => x.Identifier == vehicle.StartLane);
            if (node is null) throw new Exception($"Entry lane with ID {vehicle.StartLane} not exists");
            if (node.Type != CrossNodeType.Input) throw new Exception($"Node with ID is not a entry lane but of type {node.Type}");

            var exitNode = Map.GetAllNodes().FirstOrDefault(x => x.Identifier == vehicle.DestinationLane);
            if (exitNode is null) throw new Exception($"Exit lane with ID {vehicle.DestinationLane} not exists");
            if (exitNode.Type != CrossNodeType.Output) throw new Exception($"Node with ID is not a exit lane but of type {node.Type}");
                    
            node.Vehicle = vehicle;

        }

        public void RemoveVehicle(int vehicleId)
        {
            var node = Map.GetAllNodes().FirstOrDefault(x => x.Identifier == vehicleId);
            if (node is null) throw new Exception($"Vehicle with ID {vehicleId} not exists");
            if (node.Vehicle is null) throw new Exception($"Vehicle with ID {vehicleId} not exists");
            node.Vehicle = null;
        }


    }

}
