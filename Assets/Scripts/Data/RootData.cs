using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/RootData")]
public class RootData : ScriptableObject
{
    private static RootData _instance;

    #region Singleton

    public static RootData RootInstance
    {
        get
        {
            if (_instance == null)
            {
#if UNITY_EDITOR
                Debug.Log($"Loading... RootData instance");
#endif
                _instance = Resources.Load<RootData>("Data/RootData");
                if (_instance == null)
                {
                    Debug.LogError("RootData doesn't found");
#if UNITY_EDITOR
                    Debug.Log($"Creating... RootData instance");
#endif
                    _instance = CreateInstance<RootData>();
                }
            }

            return _instance;
        }
    }

    #endregion

    [InlineEditor, LabelText("$_roomsCount")] public LevelGenerationData LevelGenerationData;

    private string _roomsCount => $"Generated {LevelGenerationData.RoomsList.Count} Rooms";


}
