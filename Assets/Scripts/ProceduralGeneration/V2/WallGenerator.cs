using System.Collections;
using System.Collections.Generic;
using ProceduralGeneration.V2;
using Sirenix.Utilities;
using Unity.VisualScripting;
using UnityEngine;

public static class WallGenerator
{
    public static void GenerateWalls(HashSet<Vector2Int> floorPositions, TilemapVisualiser tilemapVisualiser)
    {
        var basicWallPositions = FindWallsInDirections(floorPositions, Direction2D.CardinalDirectionsList);
        foreach (var position in basicWallPositions)
        {
            tilemapVisualiser.PaintSingleBasicWall(position);
        }
    }

    private static HashSet<Vector2Int> FindWallsInDirections(HashSet<Vector2Int> floorPositions, List<Vector2Int> directionsList)
    {
        var wallPositions = new HashSet<Vector2Int>();
        foreach (var position in floorPositions)
        {
            foreach (var direction in directionsList)
            {
                var neighbourPosition = position + direction;
                if (floorPositions.Contains(neighbourPosition) == false)
                    wallPositions.Add(neighbourPosition);
            }
        }

        return wallPositions;
    }
}