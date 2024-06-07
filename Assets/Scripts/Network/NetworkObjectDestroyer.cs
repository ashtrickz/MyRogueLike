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
    
}