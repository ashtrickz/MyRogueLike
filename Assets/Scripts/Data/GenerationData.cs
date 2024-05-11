using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProceduralGeneration
{
    [CreateAssetMenu(menuName = "Data/Generation/LevelGenerationData")]
    public class GenerationData : SerializedScriptableObject
    {
        public string GenerationName = "SavedGeneration";
        public Dictionary<int, LevelGenerationData.OldRoomData> RoomsDictionary = new();
        public HashSet<Vector2Int> Corridors = new();

        [HideInInspector] public int MinPropCount;
        [HideInInspector] public int MaxPropCount;

    }
}
