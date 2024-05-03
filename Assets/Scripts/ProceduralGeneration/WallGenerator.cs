using System.Collections.Generic;
using UnityEngine;

namespace ProceduralGeneration
{
    public static class WallGenerator
    {
        public static void GenerateWalls(HashSet<Vector2Int> floorPositions, TilemapVisualiser tilemapVisualiser)
        {
            var basicWallPositions = FindWallsInDirections(floorPositions, Direction2D.CardinalDirectionsList);
            var cornerWallPositions = FindWallsInDirections(floorPositions, Direction2D.DiagonalDirectionsList);

            GenerateBasicWall(tilemapVisualiser, basicWallPositions, floorPositions);
            GenerateCornerWalls(tilemapVisualiser, cornerWallPositions, floorPositions);
        }

        private static void GenerateBasicWall(TilemapVisualiser tilemapVisualiser, HashSet<Vector2Int> wallPositions,
            HashSet<Vector2Int> floorPositions)
        {
            foreach (var position in wallPositions)
            {
                string neighboursBinaryType = string.Empty;
                foreach (var direction in Direction2D.CardinalDirectionsList)
                {
                    var neighbourPosition = position + direction;
                    neighboursBinaryType += floorPositions.Contains(neighbourPosition) ? "1" : "0";
                }

                tilemapVisualiser.PaintSingleBasicWall(position, neighboursBinaryType);
            }
        }

        private static void GenerateCornerWalls(TilemapVisualiser tilemapVisualiser, HashSet<Vector2Int> wallPositions,
            HashSet<Vector2Int> floorPositions)
        {
            foreach (var position in wallPositions)
            {
                string neighboursBinaryType = string.Empty;
                foreach (var direction in Direction2D.EightDirectionsList)
                {
                    var neighbourPosition = position + direction;
                    neighboursBinaryType += floorPositions.Contains(neighbourPosition) ? "1" : "0";
                }

                tilemapVisualiser.PaintSingleCornerWall(position, neighboursBinaryType);
            }
        }

        private static HashSet<Vector2Int> FindWallsInDirections(HashSet<Vector2Int> floorPositions,
            List<Vector2Int> directionsList)
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
}