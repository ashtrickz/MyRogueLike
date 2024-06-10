using System.Security.Cryptography;
using DungeonGeneration;
using Mirror;
using StateMachine.Player;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/EnvironmentActions/SimpleDestroy")]
public class SimpleDestroyAction : EnvironmentAction
{
    public override void OnInvoke(GameObject interactedObject)
    {
        NetworkClient.localPlayer.GetComponent<PlayerBehaviour>().DestroyNetObjectObject(interactedObject.gameObject);
    }
    
}