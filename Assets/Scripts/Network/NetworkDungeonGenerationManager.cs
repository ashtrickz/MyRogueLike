using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using DungeonGeneration;
using Mirror;
using Sirenix.OdinInspector.Editor.TypeSearch;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class NetworkDungeonGenerationManager : NetworkBehaviour
{
    public static NetworkDungeonGenerationManager Instance { get; private set; }

    private List<PropBehaviour> _spawnedPropsList = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    public DungeonGenerator Generator;

    [ClientRpc]
    public void GenerateDungeonClientRpc(int seed)
    {
        if (isServer) return;
        //Generator.GenerateDungeon(seed);
    }

    [Server]
    public void SpawnPropsServerRpc(Transform propsParent, List<Vector2> propsPositions)
    {
        SpawnPropsClientRpc(propsParent, propsPositions);
    }

    [ClientRpc]
    public void SpawnPropsClientRpc(Transform propsParent, List<Vector2> propsPositions)
    {
        _spawnedPropsList = new();

        var root = RootData.RootInstance;

        Dictionary<Vector2, PropData> propsDictionary = new();

        foreach (var position in propsPositions)
        {
            var propObject = Instantiate(RogueNetworkManager.singleton.spawnPrefabs[0], propsParent);
            if (isServer) NetworkServer.Spawn(propObject);

            var prop = propObject.GetComponent<PropBehaviour>();
            prop.transform.position = (Vector2) position;
            prop.Init(root.GetRandomPropData());
            propsDictionary.Add(position, prop.GetData());
            _spawnedPropsList.Add(prop);
        }
    }

    [Command(requiresAuthority = false)]
    public void DespawnPropsServerRpc()
    {
        DespawnPropsClientRpc();
    }

    [ClientRpc]
    private void DespawnPropsClientRpc()
    {
        foreach (var prop in _spawnedPropsList)
        {
            if (prop.gameObject == null) continue;
            NetworkObjectDestroyer.Instance.DestroyObjectServerRpc(prop.gameObject);
            
        }
    }
}