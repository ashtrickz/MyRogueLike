using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class RogueNetworkManager : NetworkManager
{
    [Space] public DungeonGenerator Generator;

    public override void OnStartServer()
    {
        base.OnStartServer();
        Generator.GenerateDungeon();
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        
    }
}
