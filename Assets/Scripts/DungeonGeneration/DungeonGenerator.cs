using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonGenerator : SerializedMonoBehaviour
{
    [HorizontalGroup("Tilemaps", .5f, Title = "References"), LabelWidth(80), SerializeField]
    private Tilemap floorTilemap, wallTilemap;

    [InlineEditor, SerializeField] private LevelTilingPreset tilingPreset;

    [Header("Generation Data"), SerializeField]
    private int roomCount;

    [HorizontalGroup("Borders", 0.5f, PaddingRight = 15)]
    [BoxGroup("Borders/Room Width"), LabelWidth(100), SerializeField]
    private int minimumWidth;

    [BoxGroup("Borders/Room Width"), LabelWidth(100), SerializeField]
    private int maximumWidth;

    [BoxGroup("Borders/Room Height"), LabelWidth(100), SerializeField]
    private int minimumHeight;

    [BoxGroup("Borders/Room Height"), LabelWidth(100), SerializeField]
    private int maximumHeight;

    [Button, HorizontalGroup("Generate Dungeon", MarginLeft = 0.05f, MarginRight = 0.025f)]
    public void GenerateDungeon()
    {
        ClearDungeon();
        // for (int i = 0; i < roomCount; i++)
        // {
        var roomWidth = Random.Range(minimumWidth, maximumWidth);
        var roomHeight = Random.Range(minimumHeight, maximumHeight);

        if (roomWidth % 2 == 0) roomWidth += 1;
        if (roomHeight % 2 == 0) roomHeight += 1;

        Vector2Int spawnPosition = new Vector2Int(0, 0);

        HashSet<Vector2Int> floor = GenerateFloor(spawnPosition, roomWidth, roomHeight);
        HashSet<Vector2Int> walls = GenerateWalls(floor, roomWidth, roomHeight);

        PaintTiles(floor, floorTilemap, tilingPreset.FloorTile);

        //}
    }

    private HashSet<Vector2Int> GenerateFloor(Vector2Int cornerSpawnPosition, int roomWidth, int roomHeight)
    {
        HashSet<Vector2Int> floor = new();

        for (int i = cornerSpawnPosition.x; i < roomWidth; i++)
        {
            for (int j = cornerSpawnPosition.y; j < roomHeight; j++)
            {
                floor.Add(new Vector2Int(i, j));
            }
        }
        
        
        return floor;
    }

    private HashSet<Vector2Int> GenerateWalls(HashSet<Vector2Int> floor, int roomWidth, int roomHeight)
    {
        HashSet<Vector2Int> walls = new();

        int maxX, maxY;
        int minX = maxX = floor.FirstOrDefault().x;
        int minY = maxY = floor.FirstOrDefault().y;

        foreach (var tile in floor)
        {
            if (tile.x < minX) minX = tile.x;
            if (tile.x > maxX) maxX = tile.x;

            if (tile.y < minY) minY = tile.y;
            if (tile.y > maxY) maxY = tile.y;
        }

        var leftDownCorner = new Vector2Int(minX - 1, minY - 1);
        var rightDownCorner = new Vector2Int(maxX + 1, minY - 1);
        var rightUpperCorner = new Vector2Int(maxX + 1, maxY + 1);
        var rightUpperUpperCorner =
            new Vector2Int(rightUpperCorner.x, rightUpperCorner.y + 1);
        var leftUpperCorner = new Vector2Int(minX - 1, maxY + 1);
        var leftUpperUpperCorner = new Vector2Int(leftUpperCorner.x, leftUpperCorner.y + 1);

        walls.Add(leftDownCorner);
        walls.Add(rightDownCorner);
        walls.Add(rightUpperCorner);
        walls.Add(rightUpperUpperCorner);
        walls.Add(leftUpperCorner);
        walls.Add(leftUpperUpperCorner);

        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.LeftDownKey], leftDownCorner);
        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.RightDownKey], rightDownCorner);
        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.RightKey], rightUpperCorner);
        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.RightUpKey], rightUpperUpperCorner);
        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.LeftKey], leftUpperCorner);
        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.LeftUpKey], leftUpperUpperCorner);

        foreach (var tile in floor)
        {
            if (tile.x == roomWidth / 2) continue;
            if (tile.y == roomHeight / 2) continue;

            if (tile.x == minX) // Left
            {
                var wall = new Vector2Int(tile.x - 1, tile.y);
                walls.Add(wall);
                PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.LeftKey], wall);
            }

            if (tile.x == maxX) // Right
            {
                var wall = new Vector2Int(tile.x + 1, tile.y);
                walls.Add(wall);
                PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.RightKey], wall);
            }

            if (tile.y == minY) // Bottom
            {
                var wall = new Vector2Int(tile.x, tile.y - 1);
                walls.Add(wall);
                PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.BottomKey], wall);
            }

            if (tile.y == maxY) // Top
            {
                var wall1 = new Vector2Int(tile.x, tile.y + 1);
                var wall2 = new Vector2Int(tile.x, tile.y + 2);

                walls.Add(wall1);
                walls.Add(wall2);

                PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.TopKey], wall1);
                PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.TopTopKey], wall2);
            }
        }


        GenerateDoorways(minX, maxX, minY, maxY, roomWidth, roomHeight);

        return walls;
    }

    private void GenerateDoorways(int minX, int maxX, int minY, int maxY, int roomWidth, int roomHeight)
    {
        var leftDoorWayUp = new Vector2Int(minX - 1, minY + roomHeight / 2 + 1);
        var leftDoorWayUpUp = new Vector2Int(leftDoorWayUp.x, leftDoorWayUp.y + 1);

        var rightDoorWayUp = new Vector2Int(maxX + 1, minY + roomHeight / 2 + 1);
        var rightDoorWayUpUp = new Vector2Int(rightDoorWayUp.x, rightDoorWayUp.y + 1);

        var leftDoorWayDown = new Vector2Int(minX - 1, minY + roomHeight / 2 - 1);
        var rightDoorWayDown = new Vector2Int(maxX + 1, minY + roomHeight / 2 - 1);

        var bottomLeftDoorWay = new Vector2Int(minX + roomWidth / 2 - 1, minY - 1);
        var bottomRightDoorWay = new Vector2Int(minX + roomWidth / 2 + 1, minY - 1);

        var topLeftDoorWay = new Vector2Int(minX + roomWidth / 2 - 1, maxY + 1);
        var topRightDoorWay = new Vector2Int(minX + roomWidth / 2 + 1, maxY + 1);

        var topLeftDoorWayUp = new Vector2Int(topLeftDoorWay.x, topRightDoorWay.y + 1);
        var topRightDoorWayUp = new Vector2Int(topRightDoorWay.x, topRightDoorWay.y + 1);

        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.LeftDoorWayUp], leftDoorWayUp);
        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.LeftDoorWayUpUp], leftDoorWayUpUp);

        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.RightDoorWayUp], rightDoorWayUp);
        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.RightDoorWayUpUp], rightDoorWayUpUp);

        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.LeftDoorWayDown], leftDoorWayDown);
        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.RightDoorWayDown], rightDoorWayDown);

        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.LeftDoorWayDown], bottomLeftDoorWay);
        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.RightDoorWayDown],
            bottomRightDoorWay);

        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.LeftDoorWayUp], topLeftDoorWay);
        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.RightDoorWayUp], topRightDoorWay);

        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.LeftDoorWayUpUp], topLeftDoorWayUp);
        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.RightDoorWayUpUp],
            topRightDoorWayUp);
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

    [Button, HorizontalGroup("Generate Dungeon", MarginLeft = 0.025f, MarginRight = 0.05f)]
    private void ClearDungeon()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
    }
}