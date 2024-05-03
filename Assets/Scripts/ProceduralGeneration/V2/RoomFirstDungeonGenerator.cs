using System.Collections;
using System.Collections.Generic;
using ProceduralGeneration.V2;
using UnityEngine;

public class RoomFirstDungeonGenerator : SimpleRandomWalkGenerator
{
    [SerializeField] private int minRoomWidth = 4, minRoomHeight = 4;

    [SerializeField] private int dungeonWidth = 20, dungeonHeight = 20;

    [SerializeField, Range(0, 10)] private int offset = 1;

    [SerializeField] private bool useRandomWalk = false;

    protected override void RunProceduralGeneration()
    {
        GenerateRooms();
    }

    private void GenerateRooms()
    {
        var roomsList = ProceduralGenerationAlgorithms.BinarySpacePartitioning(
            new BoundsInt((Vector3Int) StartPosition, new Vector3Int(dungeonWidth, dungeonHeight, 0)), minRoomWidth,
            minRoomHeight);

        HashSet<Vector2Int> floor = new();
        floor = GenerateSimpleRooms(roomsList);

        TilemapVisualiser.PaintFloorTiles(floor);
        WallGenerator.GenerateWalls(floor, TilemapVisualiser);
    }

    // ACTUAL PLACE FOR DECORATING ROOMS
    private HashSet<Vector2Int> GenerateSimpleRooms(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new();
        foreach (var room in roomsList)
        {
            for (int column = offset; column < room.size.x - offset; column++)
            {
                for (int row = offset; row < room.size.y - offset; row++)
                {
                    var position = (Vector2Int) room.min + new Vector2Int(column, row);
                    floor.Add(position);
                }
            }
        }

        return floor;
    }
}