using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "SWGData_", menuName = "ProceduralGeneration/SimpleWalkGenerationData")]
public class SimpleWalkGenerationData : ScriptableObject
{
    public int IterationCount = 10;
    public int WalkLength = 10;
    public bool StartRandomlyEachIteration = true;
}
