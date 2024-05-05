using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using Sirenix.Serialization;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace ProceduralGeneration
{
    public class RoomFirstDungeonGenerator : SimpleRandomWalkGenerator
    {
        [SerializeField] private int minRoomWidth = 4, minRoomHeight = 4;
        [SerializeField] private int dungeonWidth = 20, dungeonHeight = 20;

        [SerializeField, Range(0, 10)] private int offset = 1;
        [Space]
        [SerializeField] private bool useRandomWalk = false;
        [SerializeField] private bool useSquareShapeGeneration = false;
        
        [SerializeField, Range(0, 10)] private int minPropCount, maxPropCount;
        
        protected LevelGenerationData LevelData => RootData.RootInstance.LevelGenerationData;

        protected override void RunProceduralGeneration()
        {
            GenerateRooms();
        }

        private void GenerateRooms()
        {
            ClearRoomData();

            var roomsList = ProceduralGenerationAlgorithms.BinarySpacePartitioning(
                new BoundsInt((Vector3Int) StartPosition, new Vector3Int(dungeonWidth, dungeonHeight, 0)),
                minRoomWidth,
                minRoomHeight,
                useSquareShapeGeneration
            );

            HashSet<Vector2Int> floor = new();

            floor = useRandomWalk ? GenerateRandomWalkRooms(roomsList) : GenerateSimpleRooms(roomsList);

            List<Vector2Int> roomCenters = new();
            foreach (var room in roomsList)
            {
                roomCenters.Add((Vector2Int) Vector3Int.RoundToInt(room.center));
            }
            
            HashSet<Vector2Int> corridors = ConnectRooms(roomCenters);
            floor.UnionWith(corridors);

            TilemapVisualiser.PaintFloorTiles(floor);
            WallGenerator.GenerateWalls(floor, TilemapVisualiser);
            PropGenerator.GenerateProps(minPropCount, maxPropCount, TilemapVisualiser);
        }


        // ACTUAL PLACE FOR DECORATING ROOMS
        private HashSet<Vector2Int> GenerateRandomWalkRooms(List<BoundsInt> roomsList)
        {
            HashSet<Vector2Int> floor = new();
            for (int i = 0; i < roomsList.Count; i++)
            {
                var roomBounds = roomsList[i];
                var roomCenter =
                    new Vector2Int(Mathf.RoundToInt(roomBounds.center.x), Mathf.RoundToInt(roomBounds.center.y));
                var roomFloor = RunRandomWalk(RandomWalkData, roomCenter);

                foreach (var position in roomFloor)
                {
                    if (position.x >= (roomBounds.xMin + offset) &&
                        position.x <= (roomBounds.xMax - offset) &&
                        position.y >= (roomBounds.yMin - offset) &&
                        position.y <= (roomBounds.yMax - offset))
                    {
                        floor.Add(position);
                    }
                }
            }

            return floor;
        }

        private HashSet<Vector2Int> GenerateSimpleRooms(List<BoundsInt> roomsList)
        {
            int roomId = 0;
            HashSet<Vector2Int> floor = new();
            foreach (var room in roomsList)
            {
                HashSet<Vector2Int> singleRoomFloor = new();
                for (int column = offset; column < room.size.x - offset; column++)
                {
                    for (int row = offset; row < room.size.y - offset; row++)
                    {
                        var position = (Vector2Int) room.min + new Vector2Int(column, row);
                        singleRoomFloor.Add(position);
                        floor.Add(position);
                    }
                }

                //TODO ADD ROOM DATA HERE
                LevelData.CreateRoomData(room, (Vector2Int) Vector3Int.RoundToInt(room.center),
                    singleRoomFloor, roomId);
                roomId++;
            }

            return floor;
        }
        // TILL HERE

        private HashSet<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters)
        {
            HashSet<Vector2Int> corridors = new();
            var currentRoomCenter = roomCenters[Random.Range(0, roomCenters.Count)];
            roomCenters.Remove(currentRoomCenter);

            while (roomCenters.Count > 0)
            {
                Vector2Int closestCenter = FindClosestPoint(currentRoomCenter, roomCenters);
                roomCenters.Remove(closestCenter);
                HashSet<Vector2Int> newCorridor = GenerateCorridor(currentRoomCenter, closestCenter);

                //IncreaseOneCorridorSize(newCorridor);

                currentRoomCenter = closestCenter;
                corridors.UnionWith(newCorridor);
            }

            return corridors;
        }

        private Vector2Int FindClosestPoint(Vector2Int currentRoomCenter, List<Vector2Int> roomCenters)
        {
            var closest = Vector2Int.zero;
            var distance = float.MaxValue;
            foreach (var position in roomCenters)
            {
                float currentDistance = Vector2.Distance(position, currentRoomCenter);
                if (!(currentDistance < distance)) continue;
                distance = currentDistance;
                closest = position;
            }

            return closest;
        }

        private HashSet<Vector2Int> GenerateCorridor(Vector2Int currentRoomCenter, Vector2Int destination)
        {
            HashSet<Vector2Int> corridor = new();
            var position = currentRoomCenter;
            corridor.Add(position);

            while (position.y != destination.y)
            {
                if (destination.y > position.y)
                    position += Vector2Int.up;
                else if (destination.y < position.y)
                    position += Vector2Int.down;

                corridor.Add(position);
            }

            while (position.x != destination.x)
            {
                if (destination.x > position.x)
                    position += Vector2Int.right;
                else if (destination.x < position.x)
                    position += Vector2Int.left;

                corridor.Add(position);
            }

            return corridor;
        }

        protected override void ClearTilemap()
        {
            base.ClearTilemap();
            ClearRoomData();
        }

        private void ClearRoomData() => LevelData.ClearRoomData();
    }
}