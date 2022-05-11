using Distributed.Cross.Common.Algorithm.Utilities;
using Distributed.Cross.Common.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Distributed.Cross.Common.Algorithm
{
    public class TrajectoryAlgorithm
    {
        private CrossMap _map;
        private int _nodeNumber;

        private double[,] _graphEdgeMatrix;

        public TrajectoryAlgorithm(CrossMap map)
        {
            _map = map;
            _nodeNumber = _map.Map.GetAllNodes().Count();
        }


        public void CreateGraphMatrixRappresentation()
        {
            _graphEdgeMatrix = new double[_nodeNumber, _nodeNumber];

            foreach (var node in _map.Map.GetAllNodes())
            {
                var edges = _map.Map.GetToEdges(node);
                foreach (var edge in edges)
                {
                    //Edges to broken nodes are set to 0.
                    if (_map.BrokenNodes.Contains(edge.Tail) || _map.BrokenNodes.Contains(edge.Head))
                    {
                        _graphEdgeMatrix[node.Identifier - 1, edge.Tail.Identifier - 1] = 0;
                    }
                    else
                    {
                        _graphEdgeMatrix[node.Identifier - 1, edge.Tail.Identifier - 1] = edge.Weight;
                    }
                   
                }
            }
        }


        public TrajectoryResult Calculate(int nodeIdentifier)
        {
            var dist = Dijkstra(_graphEdgeMatrix, nodeIdentifier - 1);

            var vehicle = _map.Map.GetAllNodes().First(x => x.Identifier == nodeIdentifier).Vehicle;
            var destination = dist[vehicle.OutputLane - 1];

            var result = new TrajectoryResult
            {
                Identifier = nodeIdentifier,
            };

            //If -1 this means that no trajectory was found
            while (destination.ParentIdentifier != nodeIdentifier - 1 && destination.ParentIdentifier != -1)
            {
                result.Trajectory.Add(destination.ParentIdentifier + 1);
                destination = dist[destination.ParentIdentifier];
            }

            result.Trajectory.Reverse();
            return result;
        }



        // Function that implements Dijkstra's
        // single source shortest path algorithm
        // for a graph represented using adjacency
        // matrix representation
        private DijkstraResult[] Dijkstra(double[,] graph, int src)
        {

            var dist = new DijkstraResult[_nodeNumber];
            // The output array. dist[i]
            // will hold the shortest
            // distance from src to i

            // sptSet[i] will true if vertex
            // i is included in shortest path
            // tree or shortest distance from
            // src to i is finalized
            var sptSet = new bool[_nodeNumber];

            // Initialize all distances as
            // INFINITE and stpSet[] as false
            for (int i = 0; i < _nodeNumber; i++)
            {
                dist[i] = new DijkstraResult
                {
                    ParentIdentifier = -1,
                    Distance = double.MaxValue,
                };

                sptSet[i] = false;
            }

            // Distance of source vertex
            // from itself is always 0
            dist[src] = new DijkstraResult
            {
                Distance = 0,
                ParentIdentifier = 0,
            };

            // Find shortest path for all vertices
            for (int count = 0; count < _nodeNumber - 1; count++)
            {
                // Pick the minimum distance vertex
                // from the set of vertices not yet
                // processed. u is always equal to
                // src in first iteration.
                var u = MinDistance(dist, sptSet);

                // Mark the picked vertex as processed
                sptSet[u] = true;

                // Update dist value of the adjacent
                // vertices of the picked vertex.
                for (int v = 0; v < _nodeNumber; v++)

                    // Update dist[v] only if is not in
                    // sptSet, there is an edge from u
                    // to v, and total weight of path
                    // from src to v through u is smaller
                    // than current value of dist[v]
                    if (!sptSet[v] && graph[u, v] != 0 &&
                         dist[u].Distance != double.MaxValue && dist[u].Distance + graph[u, v] < dist[v].Distance)
                    {
                        dist[v].Distance = dist[u].Distance + graph[u, v];
                        dist[v].ParentIdentifier = u;
                    }
            }

            return dist;

        }

        private int MinDistance(DijkstraResult[] dist, bool[] sptSet)
        {
            // Initialize min value
            double min = double.MaxValue;
            int min_index = -1;

            for (int v = 0; v < _nodeNumber; v++)
                if (sptSet[v] == false && dist[v].Distance <= min)
                {
                    min = dist[v].Distance;
                    min_index = v;
                }

            return min_index;
        }


    }

}
