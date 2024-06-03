using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using DungeonGeneration;
using Mirror;
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

    [SerializeField] private NetworkDungeonGenerator dungeonGeneratorClientRpc;

    [HorizontalGroup("Tilemaps", .5f), LabelWidth(80), SerializeField]
    private Tilemap floorTilemap, wallTilemap;

    [SerializeField] private Transform propsParent;

    [Title("Generation Data", titleAlignment: TitleAlignments.Centered), InlineEditor, SerializeField]
    private DungeonGenerationData generationData;

    [SerializeField] public string GameSeed = "Default";
    [SerializeField] public int CurrentSeed = 0;
    
    [MinMaxSlider(2, 10), LabelWidth(120f), SerializeField, SuffixLabel("$_roomsCountString")]
    private Vector2Int roomCount = new(5, 7);

    [HorizontalGroup("Props"), MinMaxSlider(2, 10), LabelWidth(120f), SerializeField]
    private Vector2Int propsPerRoom = new(5, 7);

    [HorizontalGroup("Props", width: 100f), SerializeField, LabelWidth(85f)]
    private bool alignByWalls = false;

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
#endif

    public Tilemap FloorTilemap => floorTilemap;
    public Tilemap WallTilemap => wallTilemap;
    
    private int _roomCount = 0;

    public void GenerateDungeon(int seed)
    {
        CurrentSeed = seed;
        
        Random.InitState(CurrentSeed);
        
        ClearDungeon();

        GenerateRooms();
        ConnectAndPaintRooms();
    }

    [Button, HorizontalGroup("Generate Dungeon", PaddingLeft = 15f)]
    public void GenerateDungeon()
    {
        CurrentSeed = GameSeed.GetHashCode();
        Random.InitState(CurrentSeed);
        
        ClearDungeon();

        GenerateRooms();
        ConnectAndPaintRooms();

        dungeonGeneratorClientRpc.GenerateDungeonClientRpc(CurrentSeed);
    }

    private void GenerateRooms()
    {
        _roomCount = Random.Range(roomCount.x, roomCount.y + 1);

        for (int i = 0; i < _roomCount; i++)
        {
            int roomWidth, roomHeight;

            Vector2Int spawnPosition;
            Direction takenDirection;

            while (true)
            {
                roomWidth = Random.Range(minimumWidth, maximumWidth + 1);
                roomHeight = Random.Range(minimumHeight, maximumHeight + 1);

                if (roomWidth % 2 == 0) roomWidth += 1;
                if (roomHeight % 2 == 0) roomHeight += 1;

                if (i == 0)
                {
                    spawnPosition = new Vector2Int(-roomWidth / 2, -roomHeight / 2);
                    takenDirection = Direction.Bottom;
                    break;
                }

                if (RoomCanBePlaced(roomWidth, roomHeight, out spawnPosition, out takenDirection))
                    break;
            }

            GenerateSingleRoom(i, roomWidth, roomHeight, spawnPosition, takenDirection);
        }
    }

    private void GenerateSingleRoom(int roomId, int roomWidth, int roomHeight, Vector2Int spawnPosition,
        Direction takenDirection)
    {
        HashSet<Vector2Int> floor = GenerateFloor(roomWidth, roomHeight, spawnPosition);
        HashSet<Vector2Int> walls = GenerateWalls(floor, out var doorwayPoints,
            out var minX, out var maxX,
            out var minY, out var maxY);

        Dictionary<Vector2, PropData> props =
            GenerateProps(minX, maxX, minY, maxY, Random.Range(propsPerRoom.x, propsPerRoom.y + 1));

        AddRoomData(roomId, floor, walls, doorwayPoints, takenDirection, new Vector2Int(minX, maxX),
            new Vector2Int(minY, maxY), props);
    }

    private HashSet<Vector2Int> GenerateFloor(int roomWidth, int roomHeight, Vector2Int spawnPosition)
    {
        HashSet<Vector2Int> floor = new();

        for (int i = spawnPosition.x; i < spawnPosition.x + roomWidth; i++)
        for (int j = spawnPosition.y; j < spawnPosition.y + roomHeight; j++)
            floor.Add(new Vector2Int(i, j));

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

    private HashSet<Vector2Int> GenerateWalls(HashSet<Vector2Int> floor,
        out Dictionary<Direction, Vector2Int> doorwayPoints, out int minX, out int maxX, out int minY, out int maxY)
    {
        HashSet<Vector2Int> walls = new();
        minX = maxX = floor.FirstOrDefault().x;
        minY = maxY = floor.FirstOrDefault().y;

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

        return walls;
    }
    
    private void ConnectAndPaintRooms()
    {
        foreach (var roomData in generationData.RoomsDictionary)
        {
            int minX = roomData.Value.RangeX.x;
            int maxX = roomData.Value.RangeX.y;

            int minY = roomData.Value.RangeY.x;
            int maxY = roomData.Value.RangeY.y;

            int roomWidth = Mathf.Abs(maxX - minX);
            int roomHeight = Mathf.Abs(maxY - minY);

            var floor = roomData.Value.Floor;
            var walls = roomData.Value.Walls;

            List<Direction> takenDirections = new();
            foreach (var neighbourData in roomData.Value.NeighboursList)
            {
                if (neighbourData.IsTaken) takenDirections.Add(neighbourData.DoorwayDirection);
            }

            foreach (var direction in takenDirections)
            {
                switch (direction)
                {
                    case Direction.Left:
                    {
                        var leftDoorWayUp = new Vector2Int(minX - 1, minY + roomHeight / 2 + 1);
                        var leftDoorWayUpUp = new Vector2Int(leftDoorWayUp.x, leftDoorWayUp.y + 1);
                        var leftDoorWayDown = new Vector2Int(minX - 1, minY + roomHeight / 2 - 1);

                        Vector2Int doorwayTilePosition = new Vector2Int(minX - 1, minY + roomHeight / 2);

                        walls.Remove(doorwayTilePosition);
                        floor.Add(doorwayTilePosition);

                        PaintSingleTile(wallTilemap, null, doorwayTilePosition);

                        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.LeftDoorWayUp],
                            leftDoorWayUp);
                        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.LeftDoorWayUpUp],
                            leftDoorWayUpUp);
                        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.LeftDoorWayDown],
                            leftDoorWayDown);
                        break;
                    }
                    case Direction.Bottom:
                    {
                        var bottomLeftDoorWay = new Vector2Int(minX + roomWidth / 2 - 1, minY - 1);
                        var bottomRightDoorWay = new Vector2Int(minX + roomWidth / 2 + 1, minY - 1);

                        Vector2Int doorwayTilePosition = new Vector2Int(minX + roomWidth / 2, minY - 1);

                        walls.Remove(doorwayTilePosition);
                        floor.Add(doorwayTilePosition);

                        PaintSingleTile(wallTilemap, null, doorwayTilePosition);

                        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.LeftDoorWayDown],
                            bottomLeftDoorWay);
                        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.RightDoorWayDown],
                            bottomRightDoorWay);
                        break;
                    }
                    case Direction.Right:
                    {
                        var rightDoorWayUp = new Vector2Int(maxX + 1, minY + roomHeight / 2 + 1);
                        var rightDoorWayUpUp = new Vector2Int(rightDoorWayUp.x, rightDoorWayUp.y + 1);
                        var rightDoorWayDown = new Vector2Int(maxX + 1, minY + roomHeight / 2 - 1);

                        Vector2Int doorwayTilePosition = new Vector2Int(maxX + 1, minY + roomHeight / 2);

                        walls.Remove(doorwayTilePosition);
                        floor.Add(doorwayTilePosition);

                        PaintSingleTile(wallTilemap, null, doorwayTilePosition);

                        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.RightDoorWayUp],
                            rightDoorWayUp);
                        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.RightDoorWayUpUp],
                            rightDoorWayUpUp);
                        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.RightDoorWayDown],
                            rightDoorWayDown);
                        break;
                    }
                    case Direction.Top:
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

                        PaintSingleTile(wallTilemap, null, doorwayTilePosition1);
                        PaintSingleTile(wallTilemap, null, doorwayTilePosition2);

                        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.LeftDoorWayUp],
                            topLeftDoorWay);
                        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.RightDoorWayUp],
                            topRightDoorWay);

                        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.LeftDoorWayUpUp],
                            topLeftDoorWayUp);
                        PaintSingleTile(wallTilemap, tilingPreset.WallTilesDictionary[tilingPreset.RightDoorWayUpUp],
                            topRightDoorWayUp);
                        break;
                    }
                }

                PaintTiles(floor, floorTilemap, tilingPreset.FloorTile);
            }
        }
    }

    private Dictionary<Vector2, PropData> GenerateProps(int minX, int maxX, int minY, int maxY, int propCount)
    {
        Dictionary<Vector2, PropData> propsDictionary = new();

        List<Vector2> propsPositions = FindPropsPositions(minX, maxX, minY, maxY, propCount);

        var root = RootData.RootInstance;

        foreach (var position in propsPositions)
        {
            var prop = Instantiate(root.PropPrefab, propsParent);
            prop.transform.position = (Vector2) position;
            prop.Init(root.GetRandomPropData());
            propsDictionary.Add(position, prop.GetData());
        }

        return propsDictionary;
    }

    private List<Vector2> FindPropsPositions(int minX, int maxX, int minY, int maxY, int propCount)
    {
        List<Vector2> potentialPropPositions = new();
        List<Vector2> propSpawnPositions = new();

        if (alignByWalls)
        {
            for (int i = minY; i <= maxY; i++)
            {
                if (i == (minY + maxY) / 2) continue;
                potentialPropPositions.Add(new Vector2(minX + .5f, i));
                potentialPropPositions.Add(new Vector2(maxX + .5f, i));
            }

            for (int i = minX; i <= maxX; i++)
            {
                if (i == (minX + maxX) / 2) continue;
                
                var minValue = new Vector2(i + .5f, minY);
                var maxValue = new Vector2(i + .5f, maxY);
                
                if (!potentialPropPositions.Contains(minValue)) potentialPropPositions.Add(minValue);
                if (!potentialPropPositions.Contains(maxValue)) potentialPropPositions.Add(maxValue);
            }
        }
        else
        {
            for (int i = minX; i <= maxX; i++)
            {
                for (int j = minY; j <= maxY; j++)
                {
                    potentialPropPositions.Add(new Vector2(i + .5f, j));
                }
            }
        }


        for (int i = 0; i < propCount; i++)
        {
            var pickedPosition = potentialPropPositions[Random.Range(0, potentialPropPositions.Count)];
            propSpawnPositions.Add(pickedPosition);
            potentialPropPositions.Remove(pickedPosition);
        }

        return propSpawnPositions;
    }

    private bool RoomCanBePlaced(int roomWidth, int roomHeight, out Vector2Int spawnPosition,
        out Direction takenDirection)
    {
        RoomData randomRoom = GetRandomFreeRoom();
        NeighbourData randomNeighbour = GetRandomFreeNeighbour(randomRoom, out takenDirection);
        spawnPosition = GetSpawnPosition(randomNeighbour, roomWidth, roomHeight);

        for (int i = spawnPosition.x; i < spawnPosition.x + roomWidth; i++)
        for (int j = spawnPosition.y; j < spawnPosition.y + roomHeight; j++)
        {
            for (int k = -1; k < 2; k++)
            for (int l = -2; l < 3; l++)
                if (TileCanBePlaced(new Vector2Int(i + k, j + l)) == false)
                    return false;
        }

        MarkNeighbourTaken(randomNeighbour);

        return true;
    }

    private bool TileCanBePlaced(Vector2Int tilePosition)
    {
        var tile = new Vector3Int(tilePosition.x, tilePosition.y, 0);
        return wallTilemap.GetTile(tile) == null && floorTilemap.GetTile(tile) == null;
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

    private RoomData GetRandomFreeRoom() => generationData.GetRandomFreeRoom();

    private NeighbourData GetRandomFreeNeighbour(RoomData randomRoom, out Direction takenDirection) =>
        generationData.GetRandomFreeNeighbour(randomRoom, out takenDirection);

    private void MarkNeighbourTaken(NeighbourData neighbour) => generationData.MarkNeighbourTaken(neighbour);

    private void AddRoomData(int roomId, HashSet<Vector2Int> floor, HashSet<Vector2Int> walls,
        Dictionary<Direction, Vector2Int> doorwayPoints, Direction takenDirection, Vector2Int rangeX,
        Vector2Int rangeY, Dictionary<Vector2, PropData> propsDictionary) =>
        generationData.AddRoomData(roomId, floor, walls, doorwayPoints, takenDirection, rangeX, rangeY,
            propsDictionary);

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

    [Button, HorizontalGroup("Generate Dungeon", PaddingLeft = 15, MarginRight = 0.05f)]
    private void ClearDungeon()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        while (propsParent.childCount != 0)
        {
            foreach (Transform child in propsParent)
            {
                DestroyImmediate(child.gameObject);
            }
        }

        generationData = ScriptableObject.CreateInstance<DungeonGenerationData>();
    }
}