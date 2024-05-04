using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Data/LevelGenerationData")]
public class LevelGenerationData : SerializedScriptableObject
{
    public readonly Dictionary<int, RoomData> RoomsList = new();

    private List<GameObject> _identifiersList = new();
    private bool _roomIdentifierOn = false;

    [SerializeField] private GameObject identifierPrefab;

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
        if (_roomIdentifierOn)
        {
            foreach (var identifier in _identifiersList)
                DestroyImmediate(identifier);
            _identifiersList?.Clear();
        }
        else
        {
            foreach (var roomData in RoomsList)
            {
                var identifier = Instantiate(identifierPrefab);
                identifier.transform.position =
                    new Vector3(roomData.Value.Center.x + .5f, roomData.Value.Center.y + .5f, 0);
                identifier.GetComponent<SpriteRenderer>().color = roomData.Value.ColorId;
                var text = identifier.GetComponentInChildren<TMP_Text>();
                text.text = roomData.Key.ToString();
                
                text.color = (roomData.Value.ColorId.r + roomData.Value.ColorId.g + roomData.Value.ColorId.b) / 3 > 0.4 ? Color.black : Color.white;

                _identifiersList.Add(identifier);
            }
        }

        _roomIdentifierOn = !_roomIdentifierOn;
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