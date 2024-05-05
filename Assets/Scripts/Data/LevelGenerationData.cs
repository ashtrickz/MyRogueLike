using System;
using System.Collections;
using System.Collections.Generic;
using ProceduralGeneration;
using Sirenix.OdinInspector;
using TMPro;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Data/Generation/LevelGenerationData")]
public class LevelGenerationData : SerializedScriptableObject
{
    public readonly Dictionary<int, RoomData> RoomsList = new();

    [InlineEditor] public readonly List<GenerationData> SavedGenerationsDictionary = new();

    [Range(10, 100)] public float ChanceToPaintProp = 20f;
    
    private List<GameObject> _identifiersList = new();
    private GameObject _parentIdentifier;
    private bool _roomIdentifierOn = false;

    public struct RoomData
    {
        public Color ColorId;
        public BoundsInt Bounds;
        public Vector2Int Center;
        public HashSet<Vector2Int> Floor;

        public RoomData(BoundsInt bounds, Vector2Int center, HashSet<Vector2Int> floor)
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

            foreach (var roomData in RoomsList)
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
        generationData.RoomsDictionary = RoomsList;
        
        AssetDatabase.CreateAsset(generationData, $"Assets/Resources/ProceduralGeneration/GeneratedLevels/{generationName}.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        SavedGenerationsDictionary.Add(generationData);
    } 

    [Button]
    private void LoadGenerationData(string generationName)
    {
        //TODO Loading rooms from saved data;
    }
    
    public void CreateRoomData(BoundsInt bounds, Vector2Int center, HashSet<Vector2Int> floor, int roomId)
    {
        RoomsList.Add(roomId, new RoomData(bounds, center, floor));
    }

    public void ClearRoomData()
    {
        RoomsList.Clear();
    }
}