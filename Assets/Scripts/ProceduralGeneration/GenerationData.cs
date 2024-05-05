using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProceduralGeneration
{
    [CreateAssetMenu(menuName = "Data/Generation/LevelGenerationData")]
    public class GenerationData : SerializedScriptableObject
    {
        public string GenerationName = "SavedGeneration";
        public Dictionary<int, LevelGenerationData.RoomData> RoomsDictionary = new();

        public void Init(string generationName, Dictionary<int, LevelGenerationData.RoomData> roomData)
        {
            GenerationName = generationName;
            RoomsDictionary = roomData;
        }
        
    }
}
