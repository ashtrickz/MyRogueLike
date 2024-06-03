using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NetworkDungeonGenerationManager : NetworkBehaviour
{
    public DungeonGenerator Generator;
    
    [ClientRpc]
    public void GenerateDungeonClientRpc(int seed)
    {
        if (isServer) return;
        Generator.GenerateDungeon(seed);
    }
    
}
