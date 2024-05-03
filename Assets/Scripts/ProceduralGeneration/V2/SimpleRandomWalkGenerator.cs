using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProceduralGeneration.V2;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEditor.Tilemaps;
using UnityEngine;
using Random = UnityEngine.Random;

public class SimpleRandomWalkGenerator : AbstractDungeonGenerator
{
    [Space, SerializeField] protected SimpleWalkGenerationData RandomWalkData;
    
    protected override void RunProceduralGeneration()
    {
        var floorPositions = RunRandomWalk(RandomWalkData, StartPosition);
        TilemapVisualiser.Clear();
        TilemapVisualiser.PaintFloorTiles(floorPositions);
        WallGenerator.GenerateWalls(floorPositions, TilemapVisualiser);
    }

    protected HashSet<Vector2Int> RunRandomWalk(SimpleWalkGenerationData generationData, Vector2Int position)
    {
        var currentPosition = position;
        var floorPositions = new HashSet<Vector2Int>();
        
        for (int i = 0; i < generationData.IterationCount; i++)
        {
            var path = ProceduralGenerationAlgorithms.SimpleRandomWalk(currentPosition, generationData.WalkLength);
            floorPositions.UnionWith(path);
            
            if (!generationData.StartRandomlyEachIteration) continue;
            currentPosition = floorPositions.ElementAt(Random.Range(0, floorPositions.Count));
        }

        return floorPositions;
    }
}
