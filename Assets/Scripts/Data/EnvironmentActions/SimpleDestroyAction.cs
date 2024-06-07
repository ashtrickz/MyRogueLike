using Mirror;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/EnvironmentActions/SimpleDestroy")]
public class SimpleDestroyAction : EnvironmentAction
{
    public override void OnInvoke(GameObject interactedObject)
    {
        RogueNetworkManager.Instance.NetworkObjectDestroyer.DestroyObjectServerRpc(interactedObject.gameObject);
    }
    

    
}