using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using ProceduralGeneration;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Drawers;
using Sirenix.Utilities;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using RoomData = DungeonGenerationData.RoomData;
using NeighbourData = DungeonGenerationData.RoomData.NeighbourData;

public class DungeonGenerator : SerializedMonoBehaviour
{
    [Title("References", titleAlignment: TitleAlignments.Centered), InlineEditor, SerializeField]
    private LevelTilingPreset tilingPreset;

    [HorizontalGroup("Tilemaps", .5f), LabelWidth(80), SerializeField]
    private Tilemap floorTilemap, wallTilemap;

    [Title("Generation Data", titleAlignment: TitleAlignments.Centered), InlineEditor, SerializeField]
    private DungeonGenerationData generationData;

    [MinMaxSlider(2, 10), LabelWidth(145f), SerializeField, SuffixLabel("$_roomsCountString")]
    private Vector2Int roomCount = new(5, 7);

    [MinMaxSlider(1, 4), SerializeField, LabelWidth(145f), SuffixLabel("$_doorwaysCountString")]
    private Vector2Int neighboursPerRoom = new Vector2Int(1, 4);

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
    private string _doorwaysCountString => $"Min = {neighboursPerRoom.x}, Max = {neighboursPerRoom.y}";
#endif

    private int _roomCount = 0;
    private int _midRoomOffset = 2;

    [Button, HorizontalGroup("Generate Dungeon", PaddingLeft = 15f)]
    public void GenerateDungeon()
    {
        ClearDungeon();

        _roomCount = Random.Range(roomCount.x, roomCount.y + 1);

        generationData = ScriptableObject.CreateInstance<DungeonGenerationData>();

        // GenerateRoom(0);

        for (int i = 0; i < _roomCount; i++)
        {
            GenerateRoom(i);
        }
    }

    private void GenerateRoom(int roomId) //DoorwayDirection doorwayKey, Vector2Int doorwayTilePosition)
    {
        var roomWidth = Random.Range(minimumWidth, maximumWidth + 1);
        var roomHeight = Random.Range(minimumHeight, maximumHeight + 1);

        if (roomWidth % 2 == 0) roomWidth += 1;
        if (roomHeight % 2 == 0) roomHeight += 1;

        var doorwaysAmount = Random.Range(neighboursPerRoom.x, neighboursPerRoom.y + 1);
        Dictionary<Direction, Vector2Int> doorwayPoints = new();

        Direction takenDirection;
        
        HashSet<Vector2Int> floor = GenerateFloor(roomId, roomWidth, roomHeight, out takenDirection);
        HashSet<Vector2Int>
            walls = GenerateWalls(floor, roomWidth, roomHeight, doorwaysAmount,
                out doorwayPoints); //, out roomDoorways);

        PaintTiles(floor, floorTilemap, tilingPreset.FloorTile);

        //List<Prop> props = GenerateProps();
        //object props = null;
        AddRoomData(roomId, floor, walls, doorwayPoints, takenDirection); //, props);
    }

    private void AddRoomData(int roomId, HashSet<Vector2Int> floor, HashSet<Vector2Int> walls,
        Dictionary<Direction, Vector2Int> doorwayPoints, Direction takenDirection) //, object props)
    {
        generationData.AddRoomData(roomId, floor, walls, doorwayPoints, takenDirection);
    }

    private HashSet<Vector2Int> GenerateFloor(int roomId, int roomWidth, int roomHeight, out Direction takenDirection)
    {
        HashSet<Vector2Int> floor = new();

        if (roomId == 0)
        {
            for (int i = -roomWidth / 2; i < roomWidth / 2 + 1; i++)
            {
                for (int j = -roomHeight / 2; j < roomHeight / 2 + 1; j++)
                {
                    floor.Add(new Vector2Int(i, j));
                }
            }

            takenDirection = Direction.Bottom;
        }
        else
        {
            RoomData randomRoom = GetRandomFreeRoom();
            NeighbourData randomNeighbour = GetRandomFreeNeighbour(randomRoom, out takenDirection);
            Vector2Int spawnPosition = GetSpawnPosition(randomNeighbour, roomWidth, roomHeight);
            
            
            
            for (int i = spawnPosition.x; i < spawnPosition.x + roomWidth; i++)
            {
                for (int j = spawnPosition.y; j < spawnPosition.y + roomHeight; j++)
                {
                    floor.Add(new Vector2Int(i, j));
                }
            }
            
        }

        return floor;
    }

    private Vector2Int GetSpawnPosition(NeighbourData randomNeighbour, int roomWidth, int roomHeight)
    {
        switch (randomNeighbour.DoorwayDirection)
        {
            case Direction.Left:
                return new Vector2Int(randomNeighbour.DoorwayPoint.x - roomWidth - 1,
                    randomNeighbour.DoorwayPoint.y - roomHeight / 2);
            case Direction.Bottom:
                return new Vector2Int(randomNeighbour.DoorwayPoint.x - roomWidth / 2,
                    randomNeighbour.DoorwayPoint.y - roomHeight - 2);
            case Direction.Right:
                return new Vector2Int(randomNeighbour.DoorwayPoint.x + 2,
                    randomNeighbour.DoorwayPoint.y - roomHeight / 2);
            case Direction.Top:
                return new Vector2Int(randomNeighbour.DoorwayPoint.x - roomWidth / 2,
                    randomNeighbour.DoorwayPoint.y + 2);
        }
        
        return Vector2Int.zero;
    }

    private RoomData GetRandomFreeRoom() => generationData.GetRandomFreeRoom();

    private NeighbourData GetRandomFreeNeighbour(RoomData randomRoom, out Direction takenDirection) =>
        generationData.GetRandomFreeNeighbour(randomRoom, out takenDirection);

    private HashSet<Vector2Int> GenerateWalls(HashSet<Vector2Int> floor, int roomWidth, int roomHeight,
        int doorwaysAmount,
        out Dictionary<Direction, Vector2Int> doorwayPoints) //, out Dictionary<DoorwayDirection, Vector2Int> roomDoorways)
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

        doorwayPoints = FindDoorways(minX, maxX, minY, maxY);

        //GenerateDoorways(minX, maxX, minY, maxY, roomWidth, roomHeight, doorwaysAmount, floor,
        //walls, out roomDoorways);

        return walls;
    }

    private Dictionary<Direction, Vector2Int> FindDoorways(int minX, int maxX, int minY, int maxY)
    {
        Dictionary<Direction, Vector2Int> doorways = new();

        doorways.Add(Direction.Left, new Vector2Int(minX - 1, (minY + maxY) / 2));
        doorways.Add(Direction.Bottom, new Vector2Int((minX + maxX) / 2, minY - 1));
        doorways.Add(Direction.Right, new Vector2Int(maxX + 1, (minY + maxY) / 2));
        doorways.Add(Direction.Top, new Vector2Int((minX + maxX) / 2, maxY + 2));

        return doorways;
    }

    private void GenerateDoorways(int minX, int maxX, int minY, int maxY, int roomWidth, int roomHeight,
        int doorwaysAmount, HashSet<Vector2Int> floor, HashSet<Vector2Int> walls,
        out Dictionary<DoorwayDirection, Vector2Int> roomDoorways)
    {
        List<DoorwayDirection> directionsList = RandomizeDoorways(minX, maxX, minY, maxY, doorwaysAmount);
        Dictionary<DoorwayDirection, Vector2Int> doorwayDirections = new();

        if (directionsList.Contains(DoorwayDirection.Left))
        {
            var leftDoorWayUp = new Vector2Int(minX - 1, minY + roomHeight / 2 + 1);
            var leftDoorWayUpUp = new Vector2Int(leftDoorWayUp.x, leftDoorWayUp.y + 1);
            var leftDoorWayDown = new Vector2Int(minX - 1, minY + roomHeight / 2 - 1);

            Vector2Int doorwayTilePosition = new Vector2Int(minX - 1, minY + roomHeight / 2);

            walls.Remove(doorwayTilePosition);
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

            walls.Remove(doorwayTilePosition);
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

            walls.Remove(doorwayTilePosition);
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

            walls.Remove(doorwayTilePosition1);
            walls.Remove(doorwayTilePosition2);
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

    private List<DoorwayDirection> RandomizeDoorways(int minX, int maxX, int minY, int maxY, int doorwaysAmount)
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

                if (
                    doorwayDirections
                        .Contains(
                            doorwayDirection)) //|| DirectionsIsAble(minX, maxX, minY, maxY, doorwayDirection) == false)
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

    private bool DirectionsIsAble(int minX, int maxX, int minY, int maxY, DoorwayDirection doorwayDirection)
    {
        Vector2Int firstCheckPoint = Vector2Int.zero;
        Vector2Int secondCheckPoint = Vector2Int.zero;

        switch (doorwayDirection)
        {
            case DoorwayDirection.Left:
                var leftDoorway = new Vector2Int(minX - _midRoomOffset, maxY / 2);

                firstCheckPoint = new Vector2Int(leftDoorway.x, leftDoorway.y - maximumHeight / 2); // Lower Right
                secondCheckPoint =
                    new Vector2Int(leftDoorway.x - maximumWidth, leftDoorway.y + maximumHeight / 2); // Upper Left

                for (int i = firstCheckPoint.y; i < firstCheckPoint.y + maximumHeight; i++)
                {
                    var tile = new Vector2Int(firstCheckPoint.x, i);
                    var checkedTile = wallTilemap.GetTile((Vector3Int) tile);
                    if (checkedTile != null) return false;
                }

                for (int i = secondCheckPoint.y + maximumHeight; i > secondCheckPoint.y; i--)
                {
                    var tile = new Vector2Int(secondCheckPoint.x, i);
                    var checkedTile = wallTilemap.GetTile((Vector3Int) tile);
                    if (checkedTile != null) return false;
                }

                for (int i = firstCheckPoint.x; i > firstCheckPoint.x - maximumWidth; i++)
                {
                    var tile = new Vector2Int(i, firstCheckPoint.y);
                    var checkedTile = wallTilemap.GetTile((Vector3Int) tile);
                    if (checkedTile != null) return false;
                }

                for (int i = secondCheckPoint.x; i < secondCheckPoint.x + maximumWidth; i++)
                {
                    var tile = new Vector2Int(i, firstCheckPoint.y);
                    var checkedTile = wallTilemap.GetTile((Vector3Int) tile);
                    if (checkedTile != null) return false;
                }

                break;
            case DoorwayDirection.Bottom:
                var bottomDoorway = new Vector2Int(maxX / 2, minY - _midRoomOffset);
                break;
            case DoorwayDirection.Right:
                var rightDoorway = new Vector2Int(maxX + _midRoomOffset, maxY / 2);

                firstCheckPoint =
                    new Vector2Int(rightDoorway.x + maximumWidth, rightDoorway.y - maximumHeight / 2); // Lower Right
                secondCheckPoint = new Vector2Int(rightDoorway.x, rightDoorway.y + maximumHeight / 2); // Upper Left

                for (int i = firstCheckPoint.y; i < firstCheckPoint.y + maximumHeight; i++)
                {
                    var tile = new Vector2Int(firstCheckPoint.x, i);
                    var checkedTile = wallTilemap.GetTile((Vector3Int) tile);
                    if (checkedTile != null) return false;
                }

                for (int i = secondCheckPoint.y + maximumHeight; i > secondCheckPoint.y; i--)
                {
                    var tile = new Vector2Int(secondCheckPoint.x, i);
                    var checkedTile = wallTilemap.GetTile((Vector3Int) tile);
                    if (checkedTile != null) return false;
                }

                for (int i = firstCheckPoint.x; i > firstCheckPoint.x - maximumWidth; i++)
                {
                    var tile = new Vector2Int(i, firstCheckPoint.y);
                    var checkedTile = wallTilemap.GetTile((Vector3Int) tile);
                    if (checkedTile != null) return false;
                }

                for (int i = secondCheckPoint.x; i < secondCheckPoint.x + maximumWidth; i++)
                {
                    var tile = new Vector2Int(i, firstCheckPoint.y);
                    var checkedTile = wallTilemap.GetTile((Vector3Int) tile);
                    if (checkedTile != null) return false;
                }

                break;
            case DoorwayDirection.Top:
                var topDoorway = new Vector2Int(maxX / 2, maxY + _midRoomOffset);
                break;
        }

        return true;
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