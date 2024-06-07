using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using Cinemachine;
using Mirror;
using Mirror.Examples.CCU;
using Org.BouncyCastle.Asn1.Cmp;
using StateMachine.Player;
using UnityEngine;
using Random = UnityEngine.Random;

public class RogueNetworkManager : NetworkManager
{

    public static RogueNetworkManager Instance = null;

    [Space] public DungeonGenerator Generator;
    public CinemachineVirtualCamera Camera;

    public NetworkObjectDestroyer NetworkObjectDestroyer;
    
   

    public override void Awake()
    {
        base.Awake();

        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
    }

    public override void Start()
    {
        base.Start();
    }
    
    public override void OnStartServer()
    {
        base.OnStartServer();

        var netGenerator = NetworkDungeonManager.Instance;
        var generator = netGenerator.Generator;;
        netGenerator.DungeonSeed = generator.GameSeed.GetHashCode();
        Random.InitState(netGenerator.DungeonSeed); 
        netGenerator.DungeonState = Random.state;
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

}