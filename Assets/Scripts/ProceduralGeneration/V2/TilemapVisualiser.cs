using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapVisualiser : MonoBehaviour
{
    [SerializeField] protected Tilemap floorTilemap;
    [SerializeField] protected Tilemap wallTilemap;
    
    [SerializeField] private TileBase floorTile;
    [SerializeField] protected TileBase wallTile;

    public void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions)
    {
        PaintTiles(floorPositions, floorTilemap, floorTile);
    }

    private void PaintTiles(IEnumerable<Vector2Int> floorPositions, Tilemap tilemap, TileBase tile)
    {
        foreach (var position in floorPositions)
        {
            PaintSingleTile(tilemap, tile, position);
        }
    }

    private void PaintSingleTile(Tilemap tilemap, TileBase tile, Vector2Int position)
    {
        var tilePosition = tilemap.WorldToCell((Vector3Int) position);
        tilemap.SetTile(tilePosition, tile);
    }

    public void PaintSingleBasicWall(Vector2Int position)
    {
        PaintSingleTile(wallTilemap, wallTile, position);
    }

    public void Clear()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
    }
}
