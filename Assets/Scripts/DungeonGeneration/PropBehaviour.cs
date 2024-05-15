using Sirenix.OdinInspector;
using UnityEngine;

namespace DungeonGeneration
{
    public class PropBehaviour : SerializedMonoBehaviour
    {
        [Title("References", titleAlignment: TitleAlignments.Centered), SerializeField] 
        private PolygonCollider2D collider;
        [SerializeField] 
        private Rigidbody2D rigidbodyData;
        [SerializeField, LabelText("Prop")] 
        private SpriteRenderer propSprite;
        [SerializeField, LabelText("Shadow")] 
        private SpriteRenderer shadowSprite;
        
        [Title("Prop Stats", titleAlignment: TitleAlignments.Centered),SerializeField]
        private float durability;

        [Title("Prop Data", titleAlignment: TitleAlignments.Centered), InlineEditor(), SerializeField] private PropData propData;

        [Button("Load Data")]
        public void Init(PropData data)
        {
            propData = data;
            collider.points = propData.colliderPoints;
            propSprite.sprite = propData.propSprite;
            shadowSprite.sprite = propData.shadowSprite;

            durability = propData.durability;

        }

        [Button]
        public void SaveData(PropData data, bool saveOnlyCollider)
        {
            data.colliderPoints = collider.points;
            if (saveOnlyCollider) return;
            
            data.propSprite = propSprite.sprite;
            data.shadowSprite = shadowSprite.sprite;
            data.durability = propData.durability;
        }
        
    }
}
