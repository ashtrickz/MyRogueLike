using Sirenix.OdinInspector;
using UnityEngine;

namespace ProceduralGeneration
{
    public abstract class AbstractDungeonGenerator : MonoBehaviour
    {
        [Header("Base Data")]
        [SerializeField] protected TilemapVisualiser TilemapVisualiser = null;
        [SerializeField] protected Vector2Int StartPosition = Vector2Int.zero;

        [Button, LabelText("Generate")]
        public void GenerateDungeon()
        {
            TilemapVisualiser.Clear();
            RunProceduralGeneration();
        }

        protected abstract void RunProceduralGeneration();
    }
}
