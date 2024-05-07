using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Drawers;
using Sirenix.Utilities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class DungeonGenerator : SerializedMonoBehaviour
{
    [Title("References", titleAlignment: TitleAlignments.Centered), InlineEditor, SerializeField]
    private LevelTilingPreset tilingPreset;

    [HorizontalGroup("Tilemaps", .5f), LabelWidth(80), SerializeField]
    private Tilemap floorTilemap, wallTilemap;

    [Title("Generation Data", titleAlignment: TitleAlignments.Centered), MinMaxSlider(2, 10), LabelWidth(145f),
     SerializeField, SuffixLabel("$_roomsCountString")]
    private Vector2Int roomCount = new(5, 7);

    [MinMaxSlider(1, 4), SerializeField, LabelWidth(145f), SuffixLabel("$_doorwaysCountString")]
    private Vector2Int doorwaysPerRoom = new Vector2Int(1, 4);

    [SerializeField, Range(10, 100), LabelWidth(150f), SuffixLabel("% Higher chance -> less variety")]
    private float doorwaySpawnChance = 50f;

    [HorizontalGroup("Borders", 0.5f, PaddingRight = 15)]
    [BoxGroup("Borders/Room Width"), LabelWidth(100), SerializeField]
    private int minimumWidth;

    [BoxGroup("Borders/Room Width"), LabelWidth(100), SerializeField]
    private int maximumWidth;

    [BoxGroup("Borders/Room Height"), LabelWidth(100), SerializeField]
    private int minimumHeight;

    [BoxGroup("Borders/Room Height"), LabelWidth(100), SerializeField]
    private int maximumHeight;

#if UNITY_EDITOR
    private string _roomsCountString => $"Min = {roomCount.x}, Max = {roomCount.y}";
    private string _doorwaysCountString => $"Min = {doorwaysPerRoom.x}, Max = {doorwaysPerRoom.y}";
#endif

    private int _roomsLeftToGenerate = 0;

    [Button, HorizontalGroup("Generate Dungeon", PaddingLeft = 15f)]
    public void GenerateDungeon()
    {
        ClearDungeon();

        _roomsLeftToGenerate = Random.Range(roomCount.x, roomCount.y + 1);

        GenerateRoom(DoorwayDirection.ORIGIN, Vector2Int.zero);
    }

    private void GenerateRoom(DoorwayDirection doorwayKey, Vector2Int doorwayTilePosition)
    {
        var roomWidth = Random.Range(minimumWidth, maximumWidth + 1);
        var roomHeight = Random.Range(minimumHeight, maximumHeight + 1);

        if (roomWidth % 2 == 0) roomWidth += 1;
        if (roomHeight % 2 == 0) roomHeight += 1;

        var doorwaysAmount = Random.Range(doorwaysPerRoom.x, doorwaysPerRoom.y + 1);
        Dictionary<DoorwayDirection, Vector2Int> roomDoorways = new();

        Vector2Int spawnPosition = Vector2Int.zero;
        switch (doorwayKey)
        {
            case DoorwayDirection.Left:
                spawnPosition = new Vector2Int(doorwayTilePosition.x - roomWidth - 1,
                    doorwayTilePosition.y - roomHeight / 2);
                break;
            case DoorwayDirection.Bottom:
                spawnPosition = new Vector2Int(doorwayTilePosition.x - roomWidth / 2, doorwayTilePosition.y - roomHeight - 2);
                break;
            case DoorwayDirection.Right:
                spawnPosition = new Vector2Int(doorwayTilePosition.x + 2, doorwayTilePosition.y - roomHeight / 2);
                break;
            case DoorwayDirection.Top:
                spawnPosition = new Vector2Int(doorwayTilePosition.x - roomWidth / 2, doorwayTilePosition.y + 2);
                break;
            case DoorwayDirection.ORIGIN:
                break;
        }

        Debug.Log(
            $"Rooms Spawn Point {spawnPosition}, Width: {roomWidth}, Height: {roomHeight}, Doorway Tile Position: {doorwayTilePosition}");

        HashSet<Vector2Int> floor = GenerateFloor(spawnPosition, roomWidth, roomHeight);
        HashSet<Vector2Int> walls = GenerateWalls(floor, roomWidth, roomHeight, doorwaysAmount, out roomDoorways);

        Debug.Log(floor.Count);
        
        PaintTiles(floor, floorTilemap, tilingPreset.FloorTile);
        //List<Prop> props = GenerateProps();

        //AddRoomData(floor, walls, props);
        _roomsLeftToGenerate--;
        if (_roomsLeftToGenerate <= 0) return;
        // POSSIBLE RECURSION!!!
        foreach (var doorway in roomDoorways)
        {
            GenerateRoom(doorway.Key, doorway.Value); // Possible do neighbours
        }
    }

    private HashSet<Vector2Int> GenerateFloor(Vector2Int cornerSpawnPosition, int roomWidth, int roomHeight)
    {
        HashSet<Vector2Int> floor = new();

        for (int i = cornerSpawnPosition.x; i < cornerSpawnPosition.x + roomWidth; i++)
        {
            for (int j = cornerSpawnPosition.y; j < cornerSpawnPosition.y + roomHeight; j++)
            {
                floor.Add(new Vector2Int(i, j));
            }
        }

        return floor;
    }

    private HashSet<Vector2Int> GenerateWalls(HashSet<Vector2Int> floor, int roomWidth, int roomHeight,
        int doorwaysAmount, out Dictionary<DoorwayDirection, Vector2Int> roomDoorways)
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

        foreach (var tile in floor)
        {
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

        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.LeftDownKey], leftDownCorner);
        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.RightDownKey], rightDownCorner);
        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.RightKey], rightUpperCorner);
        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.RightUpKey], rightUpperUpperCorner);
        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.LeftKey], leftUpperCorner);
        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.LeftUpKey], leftUpperUpperCorner);

        GenerateDoorways(minX, maxX, minY, maxY, roomWidth, roomHeight, doorwaysAmount, floor,
            out roomDoorways);

        return walls;
    }

    private void GenerateDoorways(int minX, int maxX, int minY, int maxY, int roomWidth, int roomHeight,
        int doorwaysAmount, HashSet<Vector2Int> floor, out Dictionary<DoorwayDirection, Vector2Int> roomDoorways)
    {
        List<DoorwayDirection> directionsList = RandomizeDoorways(doorwaysAmount);
        Dictionary<DoorwayDirection, Vector2Int> doorwayDirections = new();

        if (directionsList.Contains(DoorwayDirection.Left))
        {
            var leftDoorWayUp = new Vector2Int(minX - 1, minY + roomHeight / 2 + 1);
            var leftDoorWayUpUp = new Vector2Int(leftDoorWayUp.x, leftDoorWayUp.y + 1);
            var leftDoorWayDown = new Vector2Int(minX - 1, minY + roomHeight / 2 - 1);

            Vector2Int doorwayTilePosition = new Vector2Int(minX - 1, minY + roomHeight / 2);

            // walls.Add(leftDoorWayUp);
            // walls.Add(leftDoorWayUpUp);
            // walls.Add(leftDoorWayDown);
            //
            // walls.Remove(doorwayTilePosition);
            floor.Add(doorwayTilePosition);

            doorwayDirections.Add(DoorwayDirection.Left, doorwayTilePosition);

            PaintSingleTile(wallTilemap, null, doorwayTilePosition);

            PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.LeftDoorWayUp], leftDoorWayUp);
            PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.LeftDoorWayUpUp],
                leftDoorWayUpUp);
            PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.LeftDoorWayDown],
                leftDoorWayDown);
        }

        if (directionsList.Contains(DoorwayDirection.Bottom))
        {
            var bottomLeftDoorWay = new Vector2Int(minX + roomWidth / 2 - 1, minY - 1);
            var bottomRightDoorWay = new Vector2Int(minX + roomWidth / 2 + 1, minY - 1);

            Vector2Int doorwayTilePosition = new Vector2Int(minX + roomWidth / 2, minY - 1);

            // walls.Add(bottomLeftDoorWay);
            // walls.Add(bottomRightDoorWay);
            //
            // walls.Remove(doorwayTilePosition);
            floor.Add(doorwayTilePosition);

            doorwayDirections.Add(DoorwayDirection.Bottom, doorwayTilePosition);

            PaintSingleTile(wallTilemap, null, doorwayTilePosition);

            PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.LeftDoorWayDown],
                bottomLeftDoorWay);
            PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.RightDoorWayDown],
                bottomRightDoorWay);
        }

        if (directionsList.Contains(DoorwayDirection.Right))
        {
            var rightDoorWayUp = new Vector2Int(maxX + 1, minY + roomHeight / 2 + 1);
            var rightDoorWayUpUp = new Vector2Int(rightDoorWayUp.x, rightDoorWayUp.y + 1);
            var rightDoorWayDown = new Vector2Int(maxX + 1, minY + roomHeight / 2 - 1);

            Vector2Int doorwayTilePosition = new Vector2Int(maxX + 1, minY + roomHeight / 2);

            // walls.Add(rightDoorWayUp);
            // walls.Add(rightDoorWayUpUp);
            // walls.Add(rightDoorWayDown);
            //
            // walls.Remove(doorwayTilePosition);
            floor.Add(doorwayTilePosition);

            doorwayDirections.Add(DoorwayDirection.Right, doorwayTilePosition);

            PaintSingleTile(wallTilemap, null, doorwayTilePosition);

            PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.RightDoorWayUp], rightDoorWayUp);
            PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.RightDoorWayUpUp],
                rightDoorWayUpUp);
            PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.RightDoorWayDown],
                rightDoorWayDown);
        }

        if (directionsList.Contains(DoorwayDirection.Top))
        {
            var topLeftDoorWay = new Vector2Int(minX + roomWidth / 2 - 1, maxY + 1);
            var topRightDoorWay = new Vector2Int(minX + roomWidth / 2 + 1, maxY + 1);

            var topLeftDoorWayUp = new Vector2Int(topLeftDoorWay.x, topRightDoorWay.y + 1);
            var topRightDoorWayUp = new Vector2Int(topRightDoorWay.x, topRightDoorWay.y + 1);

            Vector2Int doorwayTilePosition1 = new Vector2Int(minX + roomWidth / 2, maxY + 1);
            Vector2Int doorwayTilePosition2 = new Vector2Int(minX + roomWidth / 2, maxY + 2);

            // walls.Add(topLeftDoorWay);
            // walls.Add(topRightDoorWay);
            // walls.Add(topLeftDoorWayUp);
            // walls.Add(topRightDoorWayUp);
            //
            // walls.Remove(doorwayTilePosition1);
            // walls.Remove(doorwayTilePosition2);
            floor.Add(doorwayTilePosition1);
            floor.Add(doorwayTilePosition2);

            doorwayDirections.Add(DoorwayDirection.Top, doorwayTilePosition2);

            PaintSingleTile(wallTilemap, null, doorwayTilePosition1);
            PaintSingleTile(wallTilemap, null, doorwayTilePosition2);

            PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.LeftDoorWayUp], topLeftDoorWay);
            PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.RightDoorWayUp],
                topRightDoorWay);

            PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.LeftDoorWayUpUp],
                topLeftDoorWayUp);
            PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.RightDoorWayUpUp],
                topRightDoorWayUp);
        }

        roomDoorways = new();
        foreach (var dir in doorwayDirections)
            roomDoorways.Add(dir.Key, dir.Value);
    }

    private List<DoorwayDirection> RandomizeDoorways(int doorwaysAmount)
    {
        List<DoorwayDirection> doorwayDirections = new();

        for (int i = 0; i < doorwaysAmount; i++)
        {
            if (Random.value * 100 < doorwaySpawnChance)
            {
                var doorwayId = Random.Range(0, 4);

                var doorwayDirection = doorwayId switch
                {
                    0 => DoorwayDirection.Left,
                    1 => DoorwayDirection.Bottom,
                    2 => DoorwayDirection.Right,
                    3 => DoorwayDirection.Top,
                    _ => DoorwayDirection.Left
                };

                if (doorwayDirections.Contains(doorwayDirection))
                {
                    i--;
                    continue;
                }

                doorwayDirections.Add(doorwayDirection);
            }
            else i--;
        }

        return doorwayDirections;
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

    [Button, HorizontalGroup("Generate Dungeon", PaddingLeft = 15, MarginRight = 0.05f)]
    private void ClearDungeon()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
    }

    enum DoorwayDirection
    {
        Left,
        Bottom,
        Right,
        Top,
        ORIGIN
    }
}