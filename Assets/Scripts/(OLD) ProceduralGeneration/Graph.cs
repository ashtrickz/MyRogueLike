using System.Collections.Generic;
using UnityEngine;

namespace ProceduralGeneration
{
    public class Graph
    {
        private List<Vector2Int> graph;

        public Graph(IEnumerable<Vector2Int> vertices)
        {
            graph = new(vertices);
        }

        public List<Vector2Int> GetNeighbours4Direction(Vector2Int startPosition)
            => GetNeighbours(startPosition, Direction2D.CardinalDirectionsList);

        public List<Vector2Int> GetNeighbours8Direction(Vector2Int startPosition)
            => GetNeighbours(startPosition, Direction2D.EightDirectionsList);

        private List<Vector2Int> GetNeighbours(Vector2Int startPosition, List<Vector2Int> neighboursOffsetList)
        {
            List<Vector2Int> neighbours = new();
            foreach (var nbrDirection in neighboursOffsetList)
            {
                Vector2Int potentialNeighbour = startPosition + nbrDirection;
                if (graph.Contains(potentialNeighbour))
                    neighbours.Add(potentialNeighbour);
            }

            return neighbours;
        }
    }
}