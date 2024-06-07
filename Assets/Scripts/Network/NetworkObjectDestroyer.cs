using System;
using System.Runtime.CompilerServices;
using Mirror;
using UnityEngine;

public class NetworkObjectDestroyer : NetworkBehaviour
{

    public static NetworkObjectDestroyer Instance { get; private set; }

    public void Awake()
    {
        if (Instance == null) Instance = this;
    }


    [Server]
    public void DestroyObjectServerRpc(GameObject obj)
    {
        DestroyObjectClientRpc(obj);
    }

    [ClientRpc]
    public void DestroyObjectClientRpc(GameObject obj)
    {
        NetworkServer.Destroy(obj);
        Destroy(obj);
    }
}