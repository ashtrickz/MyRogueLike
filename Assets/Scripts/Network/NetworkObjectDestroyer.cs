using System.Runtime.CompilerServices;
using Mirror;
using UnityEngine;

public class NetworkObjectDestroyer : NetworkBehaviour
{
    [Client]
    public void TellServerToDestroyObject(GameObject obj)
    {
        if (isServer)
        {
            NetworkServer.Destroy(obj);
            return;
        }
        AskToDestroyCmd(obj);
    }

    [Command]
    public void AskToDestroyCmd(GameObject obj)
    {
        if (!isClient) NetworkServer.Destroy(obj);
    }
}