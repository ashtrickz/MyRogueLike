using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using DungeonGeneration;
using Mirror;
using Org.BouncyCastle.Asn1.Crmf;
using Sirenix.OdinInspector.Editor.TypeSearch;
using StateMachine.Player;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class NetworkDungeonManager : NetworkBehaviour
{
    public static NetworkDungeonManager Instance { get; private set; }


    private int _roomId = 0;
    private List<SessionPropData> _spawnedPropsList = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    public DungeonGenerator Generator;

    // Generation

    // [Command()]
    // public void GenerateDungeonCmd()
    // {
    //     GenerateDungeonClientRpc(Generator.CurrentSeed);
    //     Debug.Log("Generate Cmd");
    // }

    [ClientRpc]
    public void GenerateDungeonClientRpc()
    {
        Generator.GenerateDungeonOnClient(Generator.CurrentSeed);
        Debug.Log("Generate Client Rpc");
    }

    // Props

    //public void SpawnProps(Transform propsParent, List<Vector2> propsPositions) =>
    //  SpawnPropsCmd(propsParent, propsPositions);


    [Server]
    public void SpawnPropsServer(Transform propsParent, List<Vector2> propsPositions)
    {
        var root = RootData.RootInstance;

        foreach (var position in propsPositions)
        {
            var propObject = Instantiate(RogueNetworkManager.singleton.spawnPrefabs[0], propsParent);
            NetworkServer.Spawn(propObject);

            var prop = propObject.GetComponent<PropBehaviour>();
            prop.transform.position = position;
            prop.Init(root.GetRandomPropData());

            _spawnedPropsList.Add(new SessionPropData(_roomId, position, prop.GetData(), propObject));
        }

        _roomId++;
    }

    [Server]
    public void DespawnPropsServer()
    {
        if (_spawnedPropsList.Count == 0) return;
        
        _roomId = 0;

        foreach (var prop in _spawnedPropsList)
        {
            if (prop.GameObject == null) continue;
            NetworkServer.Destroy(prop.GameObject);
            Destroy(prop.GameObject);
        }
        
        _spawnedPropsList = new();
    }

    public struct SessionPropData
    {
        public int Id;
        public Vector2 Position;
        public PropData Data;
        public GameObject GameObject;
        public SessionPropData(int id, Vector2 position, PropData data, GameObject gameObject)
        {
            Id = id;
            Position = position;
            Data = data;
            GameObject = gameObject;
        }
    }
}