using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NetworkTilemapPainter : NetworkBehaviour
{
    [SyncVar]
    private List<Vector2Int> floorData;
    
    [SyncVar]
    private List<Vector2Int> wallsData;

    private Tilemap _floorRenderer, _wallsRenderer;

    [SerializeField] private LevelTilingPreset preset;
    
    private void Awake()
    {
        if (!isClientOnly) return;
        
        PaintTiles(floorData, _floorRenderer, preset.FloorTile);
        PaintTiles(wallsData, _wallsRenderer, preset.WallTilesDictionary[preset.TopKey]);
    }

    public void PaintTiles(IEnumerable<Vector2Int> floorPositions, Tilemap tilemap, TileBase tile)
    {
        foreach (var position in floorPositions)
            PaintSingleTile(tilemap, tile, position);
    }
    
    public void PaintSingleTile(Tilemap tilemap, TileBase tile, Vector2Int position)
    {
        var tilePosition = tilemap.WorldToCell((Vector3Int) position);
        tilemap.SetTile(tilePosition, tile);
    }

    public void SetDungeonData(DungeonGenerationData dungeonGenerationData, Tilemap floorRenderer, Tilemap wallsRenderer)
    {
        floorData = new List<Vector2Int>();
        wallsData = new List<Vector2Int>();

        foreach (var room in dungeonGenerationData.RoomsDictionary.Values)
        {
            floorData.AddRange(room.Floor.ToList());
            wallsData.AddRange(room.Walls.ToList());
        }

        _floorRenderer = floorRenderer;
        _wallsRenderer = wallsRenderer;
    }
}
