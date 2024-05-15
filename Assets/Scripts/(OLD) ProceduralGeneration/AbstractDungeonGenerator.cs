using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.WSA;

namespace ProceduralGeneration
{
    public abstract class AbstractDungeonGenerator : MonoBehaviour
    {
        [Header("Base Data")]
        [SerializeField] protected TilemapVisualiser TilemapVisualiser = null;
        [SerializeField] protected Vector2Int StartPosition = Vector2Int.zero;

        [Button]
        public void GenerateDungeon()
        {
            ClearTilemap();
            RunProceduralGeneration();
        }
        
        [Button]
        protected virtual void ClearTilemap() => TilemapVisualiser.Clear();

        protected abstract void RunProceduralGeneration();
    }
}
