using System;
using System.Net.NetworkInformation;
using Mirror;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DungeonGeneration
{
    public class PropBehaviour : NetworkBehaviour, IDamageable
    {
        [Title("References", titleAlignment: TitleAlignments.Centered), SerializeField] 
        private PolygonCollider2D propCollider;

        [SerializeField] 
        private Rigidbody2D rigidbodyData;

        [SerializeField, LabelText("Prop")] 
        private SpriteRenderer propSprite;

        [SerializeField, LabelText("Shadow")] 
        private SpriteRenderer shadowSprite;

        [SyncVar, SerializeField]
        private float durability;
        
        [Title("Prop Data", titleAlignment: TitleAlignments.Centered), InlineEditor(), SerializeField] private PropData propData;
        private float _healthPoints;


        public float HealthPoints { get; set; }
        public Action OnHitTakenAction { get; set; }
        public Action<GameObject> OnDeathAction { get; set; }
        public GameObject InteractionGameObject { get; set; }

        [Button("Load Data")]
        public void Init(PropData data)
        {
            propData = data;
            propCollider.points = propData.colliderPoints;
            propSprite.sprite = propData.propSprite;
            shadowSprite.sprite = propData.shadowSprite;
            
            HealthPoints = durability = propData.durability;

            InteractionGameObject = gameObject;

            foreach (var action in data.OnDeathActions)
            {
                OnDeathAction += action.OnInvoke;
            }
            //
            // OnDeathAction += Die;
            if (isServer)
            {
                NetworkServer.Spawn(gameObject, connectionToClient);
                gameObject.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
            }
        }

        // private void Die(GameObject obj)
        // {
        //     NetworkBehaviour.Destroy(obj);
        // }

        [Button]
        public void SaveData(PropData data, bool saveOnlyCollider)
        {
            data.colliderPoints = propCollider.points;
            if (saveOnlyCollider) return;
            
            data.propSprite = propSprite.sprite;
            data.shadowSprite = shadowSprite.sprite;
            data.durability = durability;
        }

        public PropData GetData() => propData;
    }
}
