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

    [SyncVar]
    public Random.State DungeonState;

    [SyncVar]
    public int DungeonSeed;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    public DungeonGenerator Generator;

    // Generation

    [ClientRpc]
    public void GenerateDungeonClientRpc()
    {
        Generator.GenerateDungeon();
    }

    // Props

    public void SpawnProps(Transform propsParent, List<Vector2> propsPositions){
        if (isServer) SpawnPropsCmd(propsParent, propsPositions);
    }

    [Command(requiresAuthority = false)]
    public void SpawnPropsCmd(Transform propsParent, List<Vector2> propsPositions)
    {

        var root = RootData.RootInstance;

        foreach (var position in propsPositions)
        {
            var propObject = Instantiate(RogueNetworkManager.singleton.spawnPrefabs[0], propsParent);
            NetworkServer.Spawn(propObject);

            var prop = propObject.GetComponent<PropBehaviour>();
            prop.transform.position = position;

            _spawnedPropsList.Add(new SessionPropData(_roomId, position, prop, root.GetRandomPropData().stringId));
        }

        _roomId++;

        InitialisePropsRpc(_spawnedPropsList);
    }

    [Server]
    public void DespawnPropsServer()
    {
        //if (_spawnedPropsList.Count == 0) return;

        _roomId = 0;

        foreach (var prop in _spawnedPropsList)
        {
            if (prop.Behaviour.gameObject == null) continue;
            NetworkServer.Destroy(prop.Behaviour.gameObject);
            Destroy(prop.Behaviour.gameObject);
        }

        _spawnedPropsList = new();
    }

    [ClientRpc]
    private void InitialisePropsRpc(List<SessionPropData> props)
    {
        foreach (var propData in props)
            propData.Behaviour.Init(RootData.RootInstance.GetPropDataByStringId(propData.StringId));
    }

    public struct SessionPropData
    {
        public int Id;
        public Vector2 Position;
        public PropBehaviour Behaviour;
        public string StringId;

        public SessionPropData(int id, Vector2 position, PropBehaviour behaviour, string stringId)
        {
            Id = id;
            Position = position;
            Behaviour = behaviour;
            StringId = stringId;
        }
    }
}