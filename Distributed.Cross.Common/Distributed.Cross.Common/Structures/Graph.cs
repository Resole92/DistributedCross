using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Distributed.Cross.Common.Module
{
    public class Graph<T>
    {

        public List<AdjacentNodes<T>> AdjacentMatrix { get; internal set; } = new List<AdjacentNodes<T>>();


        public IEnumerable<T> GetAllNodes() => AdjacentMatrix.Select(x => x.Head);

        public void AddNeighbor(T first, T second, double weight)
        {
            AddDirectNeighbor(first, second, weight);
            AddDirectNeighbor(second, first, weight);
        }

        public void AddDirectNeighbor(T first, T second, double weight)
        {

            var firstFound = AdjacentMatrix.FirstOrDefault(x => x.Head.Equals(first));


            if (firstFound == null)
            {
                AddNode(first);
                firstFound = AdjacentMatrix.First(x => x.Head.Equals(first));
            }


            var secondFound = AdjacentMatrix.FirstOrDefault(x => x.Head.Equals(second));

            if (secondFound == null)
            {
                AddNode(second);
            }

            var nodeAlreadyPresent = firstFound.Adjacents.FirstOrDefault(x => x.Equals(second));
            if (nodeAlreadyPresent == null)
            {
                firstFound.Adjacents.Add(new Edge<T>
                {
                    Head = first,
                    Tail = second,
                    Weight = weight
                });
            }

        }

        public void AddNode(T node)
        {

            var adjacentNodesFound = AdjacentMatrix.FirstOrDefault(x => x.Head.Equals(node));
            if (adjacentNodesFound != null) return;

            var adjacentNodes = new AdjacentNodes<T>(node);
            AdjacentMatrix.Add(adjacentNodes);
        }

        public void RemoveNode(T node)
        {
            var adjacentNodesFound = AdjacentMatrix.FirstOrDefault(x => x.Head.Equals(node));
            if (adjacentNodesFound != null)
            {
                AdjacentMatrix.Remove(adjacentNodesFound);
                foreach (var adjacent in AdjacentMatrix)
                {
                    var foundEdge = adjacent.Adjacents.FirstOrDefault(x => x.Tail.Equals(node));
                    adjacent.Adjacents.Remove(foundEdge);

                }
            }
        }

        public IEnumerable<T> GetToNeighbors(T node) => GetToEdges(node).Select(x => x.Tail);

        public IEnumerable<Edge<T>> GetToEdges(T node) => AdjacentMatrix.FirstOrDefault(x => x.Head.Equals(node))?.Adjacents;


        public IEnumerable<T> GetFromNeighbors(T node)
        => GetFromEdges(node).Select(x => x.Head);


        public IEnumerable<Edge<T>> GetFromEdges(T node)
        {
            foreach (var adjacent in AdjacentMatrix.Where(x => !x.Head.Equals(node)))
            {
                var nodeFound = adjacent.Adjacents.FirstOrDefault(x => x.Tail.Equals(node));
                if (nodeFound is not null) yield return nodeFound;
            }
        }

        public void RemoveNeighbors(T first, T second)
        {
            var adjacentNodesFound = AdjacentMatrix.FirstOrDefault(x => x.Head.Equals(first));

            var foundEdge = adjacentNodesFound?.Adjacents.FirstOrDefault(x => x.Tail.Equals(second));
            adjacentNodesFound?.Adjacents.Remove(foundEdge);

        }


    }

    public class AdjacentNodes<T>
    {

        public T Head { get; set; }

        public List<Edge<T>> Adjacents { get; set; } = new List<Edge<T>>();

        public AdjacentNodes() { }

        public AdjacentNodes(T head)
        {
            Head = head;
        }
    }


    public class Edge<T>
    {
        public T Head { get; set; }
        public T Tail { get; set; }
        public double Weight { get; set; }
    }
}
