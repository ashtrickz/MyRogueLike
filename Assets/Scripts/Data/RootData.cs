using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DungeonGeneration;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using Input = UnityEngine.Windows.Input;

[CreateAssetMenu(menuName = "Data/RootData")]
public class RootData : SerializedScriptableObject
{
    #region Singleton

    private static RootData _instance;
    
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

    public InputActionAsset PlayerControls;
    
    public PropBehaviour PropPrefab;
    public List<PropData> PropsList = new();

    public PropData GetRandomPropData() => 
        PropsList[Random.Range(0, PropsList.Count)];

    public PropData GetPropDataByStringId(string propDataStringId) =>
        PropsList.Single(s => s.stringId == propDataStringId);
}