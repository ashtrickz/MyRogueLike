using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ProceduralGeneration
{
    public class TilemapVisualiser : MonoBehaviour
    {
        [SerializeField] protected Tilemap floorTilemap;
        [SerializeField] protected Tilemap wallTilemap;

        [SerializeField] private TileBase floorTile;
        [SerializeField] protected TileBase wallTop;
        [SerializeField] protected TileBase wallSideRight;
        [SerializeField] protected TileBase wallSideLeft;
        [SerializeField] protected TileBase wallBottom;
        [SerializeField] protected TileBase wallFull;
        [SerializeField] protected TileBase wallInnerDownLeftCorner, wallInnerDownRightCorner;
        [SerializeField] protected TileBase wallOuterDownLeftCorner, wallOuterDownRightCorner;
        [SerializeField] protected TileBase wallOuterUpLeftCorner, wallOuterUpRightCorner;

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

        public void PaintSingleBasicWall(Vector2Int position, string binaryType)
        {
            TileBase tile = null;
            int typeAsInt = Convert.ToInt32(binaryType, 2);

            if (WallByteTypes.wallTop.Contains(typeAsInt))
                tile = wallTop;
            else if (WallByteTypes.wallSideRight.Contains(typeAsInt))
                tile = wallSideRight;
            else if (WallByteTypes.wallSideLeft.Contains(typeAsInt))
                tile = wallSideLeft;
            else if (WallByteTypes.wallBottm.Contains(typeAsInt))
                tile = wallBottom;
            else if (WallByteTypes.wallFull.Contains(typeAsInt))
                tile = wallFull;
        
            if (tile != null)
                PaintSingleTile(wallTilemap, tile, position);
        }

        public void PaintSingleCornerWall(Vector2Int position, string binaryType)
        {
            TileBase tile = null;
            int typeAsInt = Convert.ToInt32(binaryType, 2);
        
            if (WallByteTypes.wallInnerCornerDownLeft.Contains(typeAsInt))
                tile = wallInnerDownLeftCorner;
            else if (WallByteTypes.wallInnerCornerDownRight.Contains(typeAsInt))
                tile = wallInnerDownRightCorner;
            else if (WallByteTypes.wallDiagonalCornerDownLeft.Contains(typeAsInt))
                tile = wallOuterDownLeftCorner;
            else if (WallByteTypes.wallDiagonalCornerDownRight.Contains(typeAsInt))
                tile = wallOuterDownRightCorner;
            else if (WallByteTypes.wallDiagonalCornerUpLeft.Contains(typeAsInt))
                tile = wallOuterUpLeftCorner;        
            else if (WallByteTypes.wallDiagonalCornerUpRight.Contains(typeAsInt))
                tile = wallOuterUpRightCorner;
            else if (WallByteTypes.wallFullEightDirections.Contains(typeAsInt))
                tile = wallFull;        
            else if (WallByteTypes.wallBottmEightDirections.Contains(typeAsInt))
                tile = wallBottom;
        
            if (tile != null)
                PaintSingleTile(wallTilemap, tile, position);
        }

        public void Clear()
        {
            floorTilemap.ClearAllTiles();
            wallTilemap.ClearAllTiles();
        }
    }
}