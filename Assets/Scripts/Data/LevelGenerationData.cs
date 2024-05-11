using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProceduralGeneration;
using Sirenix.OdinInspector;
using TMPro;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Data/Generation/LevelGenerationData")]
public class LevelGenerationData : SerializedScriptableObject
{
    [Range(10, 100)] public float ChanceToPaintProp = 20f;
    
    public readonly Dictionary<int, OldRoomData> RoomsDictionary = new();
    public readonly HashSet<Vector2Int> Corridors = new();
    
    private List<GameObject> _identifiersList = new();
    private GameObject _parentIdentifier;
    private bool _roomIdentifierOn = false;
    private int _minPropCount, _maxPropCount;

    public struct OldRoomData
    {
        public Color ColorId;
        public BoundsInt Bounds;
        public Vector2Int Center;
        public HashSet<Vector2Int> Floor;

        public OldRoomData(BoundsInt bounds, Vector2Int center, HashSet<Vector2Int> floor)
        {
            Bounds = bounds;
            Center = center;
            Floor = floor;
            ColorId = Random.ColorHSV();
        }
    }

    [Button]
    private void ToggleRoomsIdentifiers()
    {
        var identifierPrefab = RootData.RootInstance.IdentifierPrefab;
        if (_roomIdentifierOn)
        {
            foreach (var identifier in _identifiersList)
                DestroyImmediate(identifier);
            _identifiersList?.Clear();
        }
        else
        {
            _parentIdentifier = new GameObject("Identifiers");
            _identifiersList.Add(_parentIdentifier);

            foreach (var roomData in RoomsDictionary)
            {
                var identifier = Instantiate(identifierPrefab, _parentIdentifier.transform);
                identifier.transform.position =
                    new Vector3(roomData.Value.Center.x + .5f, roomData.Value.Center.y + .5f, 0);
                identifier.GetComponent<SpriteRenderer>().color = roomData.Value.ColorId;
                var text = identifier.GetComponentInChildren<TMP_Text>();
                text.text = roomData.Key.ToString();

                text.color = (roomData.Value.ColorId.r + roomData.Value.ColorId.g + roomData.Value.ColorId.b) / 3 > 0.4
                    ? Color.black
                    : Color.white;

                _identifiersList.Add(identifier);
            }
        }

        _roomIdentifierOn = !_roomIdentifierOn;
    }

    [Button]
    private void SaveGenerationData(string generationName)
    {
        var generationData = CreateInstance<GenerationData>();
        generationData.GenerationName = generationName;
        foreach (var roomData in RoomsDictionary)
        {
            generationData.RoomsDictionary.Add(roomData.Key, roomData.Value);
        }

        generationData.Corridors.UnionWith(Corridors);
        generationData.MinPropCount = _minPropCount;
        generationData.MaxPropCount = _maxPropCount;

        AssetDatabase.CreateAsset(generationData,
            $"Assets/Resources/ProceduralGeneration/GeneratedLevels/{generationName}.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [Button]
    private void LoadGenerationData(GenerationData generationData)
    {
        ClearLevelData();

        var tilemapVisualiser = FindObjectOfType<TilemapVisualiser>();
        HashSet<Vector2Int> floor = new();

        foreach (var roomData in generationData.RoomsDictionary.Values)
        {
            floor.UnionWith(roomData.Floor);
        }

        floor.UnionWith(generationData.Corridors);

        foreach (var room in generationData.RoomsDictionary)
        {
            RoomsDictionary.Add(room.Key, room.Value);
        }

        tilemapVisualiser.PaintFloorTiles(floor);
        WallGenerator.GenerateWalls(floor, tilemapVisualiser);
        PropGenerator.GenerateProps(generationData.MinPropCount, generationData.MaxPropCount, tilemapVisualiser);
    }

    public void CreateRoomData(BoundsInt bounds, Vector2Int center, HashSet<Vector2Int> floor, int roomId)
    {
        RoomsDictionary.Add(roomId, new OldRoomData(bounds, center, floor));
    }

    public void CompleteLevelData(HashSet<Vector2Int> corridors, int minPropCount, int maxPropCount)
    {
        foreach (var corridor in corridors)
        {
            Corridors.Add(corridor);
        }

        _minPropCount = minPropCount;
        _maxPropCount = maxPropCount;
    }

    public void ClearLevelData()
    {
        RoomsDictionary.Clear();
        Corridors.Clear();
    }
}