using UnityEngine;

namespace ProceduralGeneration
{
    [CreateAssetMenu(fileName = "SWGData_", menuName = "ProceduralGeneration/SimpleWalkGenerationData")]
    public class SimpleWalkGenerationData : ScriptableObject
    {
        public int IterationCount = 10;
        public int WalkLength = 10;
        public bool StartRandomlyEachIteration = true;
    }
}
