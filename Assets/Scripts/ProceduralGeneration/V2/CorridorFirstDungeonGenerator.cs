using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProceduralGeneration.V2;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CorridorFirstDungeonGenerator : SimpleRandomWalkGenerator
{
    [SerializeField] private int corridorLength = 14;
    [SerializeField] private int corridorCount = 5;

    [SerializeField, Range(0.1f, 1)] private float roomPercent = 0.8f;

    [SerializeField] private CorridorWidthType _corridorWidthType;
    
    protected override void RunProceduralGeneration()
    {
        GenerateFirstCorridor();
    }

    private void GenerateFirstCorridor()
    {
        HashSet<Vector2Int> floorPositions = new();
        HashSet<Vector2Int> potentialRoomPositions = new();

        var corridors = GenerateCorridors(floorPositions, potentialRoomPositions);

        HashSet<Vector2Int> roomPositions = GenerateRooms(potentialRoomPositions);
        List<Vector2Int> deadEnds = FindAllDeadEnds(floorPositions);

        GenerateRoomsAtDeadEnds(deadEnds, roomPositions);

        floorPositions.UnionWith(roomPositions);

        for (int i = 0; i < corridors.Count; i++)
        {
            switch (_corridorWidthType)
            {
                case CorridorWidthType.IncreaseByOne:
                    corridors[i] = IncreaseCorridorSizeByOne(corridors[i]);
                    break;
                case CorridorWidthType.IncreaseBrush3By3:
                    corridors[i] = IncreaseCorridorBrush3By3(corridors[i]);
                    break;
                case CorridorWidthType.Default:
                    
                    break;
            }
            floorPositions.UnionWith(corridors[i]);
        }

        TilemapVisualiser.PaintFloorTiles(floorPositions);
        WallGenerator.GenerateWalls(floorPositions, TilemapVisualiser);
    }

    private List<Vector2Int> IncreaseCorridorSizeByOne(List<Vector2Int> corridor)
    {
        List<Vector2Int> newCorridor = new();
        Vector2Int previousDirection = Vector2Int.zero;
        for (int i = 1; i < corridor.Count; i++)
        {
            Vector2Int directionFromCell = corridor[i] - corridor[i - 1];
            if (previousDirection != Vector2Int.zero &&
                directionFromCell != previousDirection)
            {
                // Handle corner
                for (int x = -1; x < 2; x++)
                {
                    for (int y = -1; y < 2; y++)
                    {
                        newCorridor.Add(corridor[i - 1] + new Vector2Int(x, y));
                    }
                }

                previousDirection = directionFromCell;
            }
            else
            {
                // Add a single cell in the direction + 90 degrees
                Vector2Int newCorridorTileOffset = GetDirection90From(directionFromCell);
                newCorridor.Add(corridor[i - 1]);
                newCorridor.Add(corridor[i - 1] + newCorridorTileOffset);

                previousDirection = directionFromCell;
            }
        }

        return newCorridor;
    }

    private List<Vector2Int> IncreaseCorridorBrush3By3(List<Vector2Int> corridor)
    {
        List<Vector2Int> newCorridor = new();
        for (int i = 1; i < corridor.Count; i++)
        {
            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    newCorridor.Add(corridor[i - 1] + new Vector2Int(x, y));
                }
            }
        }

        return newCorridor;
    }

    private Vector2Int GetDirection90From(Vector2Int directionFromCell)
    {
        if (directionFromCell == Vector2Int.up)
            return Vector2Int.right;
        if (directionFromCell == Vector2Int.right)
            return Vector2Int.down;
        if (directionFromCell == Vector2Int.down)
            return Vector2Int.left;
        if (directionFromCell == Vector2Int.left)
            return Vector2Int.up;

        return Vector2Int.zero;
    }

    private void GenerateRoomsAtDeadEnds(List<Vector2Int> deadEnds, HashSet<Vector2Int> roomFloors)
    {
        foreach (var position in deadEnds)
        {
            if (roomFloors.Contains(position) == false)
            {
                var room = RunRandomWalk(RandomWalkData, position);
                roomFloors.UnionWith(room);
            }
        }
    }

    private List<Vector2Int> FindAllDeadEnds(HashSet<Vector2Int> floorPositions)
    {
        List<Vector2Int> deadEnds = new();
        foreach (var floorPosition in floorPositions)
        {
            int neighboursCount = 0;
            foreach (var direction in Direction2D.CardinalDirectionsList)
            {
                if (floorPositions.Contains(floorPosition + direction))
                    neighboursCount++;
            }

            if (neighboursCount == 1)
                deadEnds.Add(floorPosition);
        }

        return deadEnds;
    }

    private HashSet<Vector2Int> GenerateRooms(HashSet<Vector2Int> potentialRoomPositions)
    {
        HashSet<Vector2Int> roomPositions = new();
        var roomsToCreateCount = Mathf.RoundToInt(potentialRoomPositions.Count * roomPercent);

        List<Vector2Int> roomsToCreate =
            potentialRoomPositions.OrderBy(x => Guid.NewGuid()).Take(roomsToCreateCount).ToList();

        foreach (var roomPosition in roomsToCreate)
        {
            var roomFloor = RunRandomWalk(RandomWalkData, roomPosition);
            roomPositions.UnionWith(roomFloor);
        }

        return roomPositions;
    }

    private List<List<Vector2Int>> GenerateCorridors(HashSet<Vector2Int> floorPositions,
        HashSet<Vector2Int> potentialRoomPositions)
    {
        List<List<Vector2Int>> corridors = new();
        var currentPosition = StartPosition;
        potentialRoomPositions.Add(currentPosition);

        for (int i = 0; i < corridorCount; i++)
        {
            var corridor = ProceduralGenerationAlgorithms.RandomWalkCorridor(currentPosition, corridorLength);
            corridors.Add(corridor);
            currentPosition = corridor[^1];
            potentialRoomPositions.Add(currentPosition);
            floorPositions.UnionWith(corridor);
        }

        return corridors;
    }

    public enum CorridorWidthType
    {
        IncreaseByOne,
        IncreaseBrush3By3,
        Default
    }
    
}