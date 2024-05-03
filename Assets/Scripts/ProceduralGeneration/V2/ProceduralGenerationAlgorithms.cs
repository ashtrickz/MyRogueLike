using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralGeneration.V2
{
    public static class ProceduralGenerationAlgorithms
    {
        public static HashSet<Vector2Int> SimpleRandomWalk(Vector2Int startPosition, int walkLenght)
        {
            var path = new HashSet<Vector2Int>();
            var previousPosition = startPosition;

            path.Add(startPosition);
            for (int i = 0; i < walkLenght; i++)
            {
                var newPosition = previousPosition + Direction2D.GetRandomCardinalDirection();
                path.Add(newPosition);
                previousPosition = newPosition;
            }

            return path;
        }

        public static List<Vector2Int> RandomWalkCorridor(Vector2Int startPosition, int corridorLength)
        {
            var corridor = new List<Vector2Int>();
            var direction = Direction2D.GetRandomCardinalDirection();
            var currentPosition = startPosition;
            corridor.Add(currentPosition);

            for (int i = 0; i < corridorLength; i++)
            {
                currentPosition += direction;
                corridor.Add(currentPosition);
            }

            return corridor;
        }

        public static List<BoundsInt> BinarySpacePartitioning(BoundsInt spaceToSplit, int minWidth, int minHeight)
        {
            Queue<BoundsInt> roomsQueue = new();
            List<BoundsInt> roomsList = new();
            roomsQueue.Enqueue(spaceToSplit);

            while (roomsQueue.Count > 0)
            {
                var room = roomsQueue.Dequeue();
                if (room.size.y >= minHeight && room.size.x >= minWidth)
                {
                    if (Random.value < .5f)
                    {
                        if (room.size.y >= minHeight * 2)
                            SplitHorizontally(minHeight, roomsQueue, room);
                        else if (room.size.x >= minWidth * 2)
                            SplitVertically(minWidth, roomsQueue, room);
                        else if (room.size.x >= minWidth && room.size.y >= minHeight)
                            roomsList.Add(room);
                    }
                    else
                    { 
                        if (room.size.x >= minWidth * 2)
                            SplitVertically(minWidth, roomsQueue, room);
                        else if (room.size.y >= minHeight * 2)
                            SplitHorizontally(minHeight, roomsQueue, room);
                        else if (room.size.x >= minWidth && room.size.y >= minHeight)
                            roomsList.Add(room);
                    }
                }
            }

            return roomsList;
        }

        private static void SplitVertically(int minWidth, Queue<BoundsInt> roomsQueue, BoundsInt room)
        {
            var xSplit = Random.Range(1, room.size.y); //Random.Range(minWidth, room.size.x - minWidth);
            BoundsInt room1 = new(room.min, new Vector3Int(xSplit, room.size.y, room.size.z));
            BoundsInt room2 = new(new Vector3Int(room.min.x + xSplit, room.min.y, room.min.z), 
                new Vector3Int(room.size.x - xSplit, room.size.y, room.size.z));
            
            roomsQueue.Enqueue(room1);
            roomsQueue.Enqueue(room2);
        }

        private static void SplitHorizontally(int minHeight, Queue<BoundsInt> roomsQueue, BoundsInt room)
        {
            var ySplit = Random.Range(1, room.size.y); //Random.Range(minHeight, room.size.y - minHeight); // minHeight, Room.size.y - minHeight
            BoundsInt room1 = new(room.min, new Vector3Int(room.size.x, ySplit, room.size.z));
            BoundsInt room2 = new(new Vector3Int(room.min.x, room.min.y + ySplit, room.min.z),
                new Vector3Int(room.size.x, room.size.y - ySplit, room.size.z));
            
            roomsQueue.Enqueue(room1);
            roomsQueue.Enqueue(room2);
        }
    }

    public static class Direction2D
    {
        public static readonly List<Vector2Int> CardinalDirectionsList = new()
        {
            new Vector2Int(0, 1),   // Up
            new Vector2Int(1, 0),   // Right
            new Vector2Int(0, -1),  // Down
            new Vector2Int(-1, 0)   // Left
        };        
        
        public static readonly List<Vector2Int> DiagonalDirectionsList = new()
        {
            new Vector2Int(1, 1),   // Up-Right
            new Vector2Int(1, -1),  // Down-Right
            new Vector2Int(-1, -1), // Down-Left    
            new Vector2Int(-1, 1)   // Up-Left
        };

        public static readonly List<Vector2Int> EightDirectionsList = new()
        {
            new Vector2Int(0, 1),   // Up
            new Vector2Int(1, 1),   // Up-Right
            new Vector2Int(1, 0),   // Right
            new Vector2Int(1, -1),  // Down-Right
            new Vector2Int(0, -1),  // Down
            new Vector2Int(-1, -1), // Down-Left 
            new Vector2Int(-1, 0),  // Left
            new Vector2Int(-1, 1)   // Up-Left
        };
        
        public static Vector2Int GetRandomCardinalDirection() =>
            CardinalDirectionsList[UnityEngine.Random.Range(0, CardinalDirectionsList.Count)];
    }
}