using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Distributed.Cross.Common.Module
{
    public class CrossMap
    {

        public Graph<CrossNode> Map { get; set; } = new Graph<CrossNode>();

        public int Height { get; private set; }
        public int Lenght { get; private set; }

        public CrossMap(int height, int lenght)
        {
            Height = height;
            Lenght = lenght;
            BuildIdentifier();
        }

        public CrossNode[,] BuildInternalMap()
        {

            var crossMatrix = new CrossNode[Height, Lenght];

            for (int row = 0; row <= Height; row++)
            {
                for (int column = 0; column <= Lenght; column++)
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

            foreach(var cross in Map.GetAllNodes().Where(x => x.Type == CrossNodeType.Cross))
            {
                cross.Identifier = count++;
            }
        }


        /// <summary>
        /// Valid only if node choose exists and is on perimeter
        /// </summary>
        /// <param name="externalNodeId"></param>
        public void AddInputLane(int rowCell, int columnCell, CrossNode[,] crossMatrix)
        {

            var inputNode = new CrossNode
            {
                Type = CrossNodeType.Input
            };

            var targetNode = crossMatrix[rowCell, columnCell];
            Map.AddDirectNeighbor(inputNode, targetNode,1);

        }

        /// <summary>
        /// Valid only if node choose exists and is on perimeter
        /// </summary>
        /// <param name="externalNodeId"></param>
        public void AddOutputLane(int rowCell, int columnCell, CrossNode[,] crossMatrix)
        {
            var outputNode = new CrossNode
            {
                Type = CrossNodeType.Output
            };

            var targetNode = crossMatrix[rowCell, columnCell];
            Map.AddDirectNeighbor(targetNode, outputNode, 1);
        }





    }

}
