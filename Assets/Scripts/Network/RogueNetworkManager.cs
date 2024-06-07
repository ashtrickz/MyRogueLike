using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using Cinemachine;
using Mirror;
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
        StartCoroutine(WaitTillPlayersReadyCo());
    }

    private IEnumerator WaitTillPlayersReadyCo()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("Time waited");
        NetworkDungeonManager.Instance.GenerateDungeonCmd();
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