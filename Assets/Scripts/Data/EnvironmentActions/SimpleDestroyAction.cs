using Mirror;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/EnvironmentActions/SimpleDestroy")]
public class SimpleDestroyAction : EnvironmentAction
{
    public override void OnInvoke(GameObject interactedObject)
    {
        //NetworkDungeonManager.Instance.DestroyObjectRpc(interactedObject.gameObject);
    }
    

    
}