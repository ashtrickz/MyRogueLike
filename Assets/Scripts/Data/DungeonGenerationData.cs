using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProceduralGeneration;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using RoomData = DungeonGenerationData.RoomData;
using NeighbourData = DungeonGenerationData.RoomData.NeighbourData;

[CreateAssetMenu(menuName = "Data/Generation/DungeonGenerationData")]
public class DungeonGenerationData : SerializedScriptableObject
{
    public Dictionary<int, RoomData> RoomsDictionary;

    private void Awake()
    {
        RoomsDictionary = new();
    }

    public void AddRoomData(int roomId, HashSet<Vector2Int> floor, HashSet<Vector2Int> walls,
        Dictionary<Direction, Vector2Int> doorwayPoints, Direction takenDirection, Vector2Int rangeX, Vector2Int rangeY)
    {
        RoomsDictionary.Add(roomId, new RoomData(floor, walls, doorwayPoints, takenDirection, rangeX, rangeY));
    }

    public RoomData GetRandomFreeRoom()
    {
        List<RoomData> freeRooms = new();

        foreach (var roomData in RoomsDictionary)
            if (roomData.Value.HasFreeNeighbour())
                freeRooms.Add(roomData.Value);

        // TODO Implement NULL exceptions 
        RoomData selectedRoom = freeRooms[Random.Range(0, freeRooms.Count)];
        return selectedRoom;
    }

    public NeighbourData GetRandomFreeNeighbour(RoomData randomRoom, out Direction takenDirection)
    {
        List<NeighbourData> freeNeighbours = new();

        foreach (var neighbourData in randomRoom.NeighboursList)
            if (!neighbourData.IsTaken)
                freeNeighbours.Add(neighbourData);

        // TODO Implement NULL exceptions 
        NeighbourData selectedNeighbour = freeNeighbours[Random.Range(0, freeNeighbours.Count)];

        takenDirection = Direction.Bottom;

        switch (selectedNeighbour.DoorwayDirection)
        {
            case Direction.Bottom:
                takenDirection = Direction.Top;
                break;
            case Direction.Left:
                takenDirection = Direction.Right;
                break;
            case Direction.Right:
                takenDirection = Direction.Left;
                break;
            case Direction.Top:
                takenDirection = Direction.Bottom;
                break;
        }

        return selectedNeighbour;
    }

    public void MarkNeighbourTaken(NeighbourData selectedNeighbour)
    {
        foreach (var roomData in RoomsDictionary.Values)
        {
            if (roomData.ContainsNeighbour(selectedNeighbour))
            {
                roomData.MarkNeighbourAsTaken(selectedNeighbour);
            }
        }
    }

    public struct RoomData
    {
        [field: SerializeField, DisplayAsString]
        public HashSet<Vector2Int> Floor { get; private set; }

        [field: SerializeField, DisplayAsString]
        public HashSet<Vector2Int> Walls { get; private set; }

        [field: SerializeField, DisplayAsString] public Vector2Int RangeX { get; private set; }
        [field: SerializeField, DisplayAsString] public Vector2Int RangeY { get; private set; }
        
        [field: SerializeField] public List<NeighbourData> NeighboursList { get; private set; }

        public RoomData(HashSet<Vector2Int> floor, HashSet<Vector2Int> walls,
            Dictionary<Direction, Vector2Int> doorwayPoints, Direction takenDirection, Vector2Int rangeX, Vector2Int rangeY)
        {
            Floor = floor;
            Walls = walls;

            RangeX = rangeX;
            RangeY = rangeY;
            
            NeighboursList = new();
            foreach (var doorwayPoint in doorwayPoints)
                NeighboursList.Add(new NeighbourData(doorwayPoint.Key, doorwayPoint.Value));
            MarkNeighbourAsTaken(NeighboursList.Single(dir => dir.DoorwayDirection == takenDirection));
        }

        public bool HasFreeNeighbour()
        {
            foreach (var neighbourData in NeighboursList)
                if (!neighbourData.IsTaken)
                    return true;

            return false;
        }

        public bool ContainsNeighbour(NeighbourData neighbourData) => NeighboursList.Contains(neighbourData);

        public void MarkNeighbourAsTaken(NeighbourData neighbourData)
        {
            List<NeighbourData> updatedNeighbours = new();

            foreach (var neighbour in NeighboursList)
            {
                if (neighbour.DoorwayDirection == neighbourData.DoorwayDirection)
                    neighbour.MarkTaken();

                updatedNeighbours.Add(neighbour);
            }

            NeighboursList.Clear();
            NeighboursList.AddRange(updatedNeighbours);
        }

        public struct NeighbourData
        {
            [field: SerializeField, DisplayAsString]
            public Vector2Int DoorwayPoint { get; private set; }

            [field: SerializeField, DisplayAsString]
            public Direction DoorwayDirection { get; private set; }

            [field: SerializeField, DisplayAsString]
            public bool IsTaken { get; private set; }

            public NeighbourData(Direction doorwayDirection, Vector2Int doorwayPoint)
            {
                DoorwayPoint = doorwayPoint;
                DoorwayDirection = doorwayDirection;
                IsTaken = false;
            }

            public void MarkTaken() => IsTaken = true;
        }
    }
}