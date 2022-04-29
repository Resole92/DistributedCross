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

        public void AddNeighbor(T first, T second)
        {
            AddDirectNeighbor(first, second);
            AddDirectNeighbor(second, first);
        }

        public void AddDirectNeighbor(T first, T second)
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
                firstFound.Adjacents.Add(second);
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
                    adjacent.Adjacents.Remove(node);
                }
            }
        }

        public IEnumerable<T> GetToNeighbors(T node) => AdjacentMatrix.FirstOrDefault(x => x.Head.Equals(node))?.Adjacents;


        public IEnumerable<T> GetFromNeighbors(T node)
        {
            foreach (var adjacent in AdjacentMatrix.Where(x => !x.Head.Equals(node)))
            {
                if (adjacent.Adjacents.Contains(node)) yield return adjacent.Head;
            }
        }

        public void RemoveNeighbors(T first, T second)
        {
            var adjacentNodesFound = AdjacentMatrix.FirstOrDefault(x => x.Head.Equals(first));
            adjacentNodesFound?.Adjacents.Remove(second);
        }


    }

    public class AdjacentNodes<T>
    {

        public T Head { get; set; }

        public List<T> Adjacents { get; set; } = new List<T>();

        public AdjacentNodes() { }

        public AdjacentNodes(T head)
        {
            Head = head;
        }
    }
}
