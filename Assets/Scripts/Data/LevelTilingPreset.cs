using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Data/LevelTilingPreset")]
public class LevelTilingPreset : SerializedScriptableObject
{
    public TileBase FloorTile;
    public Dictionary<string, TileBase> WallTilesDictionary = new();

    [NonSerialized] public readonly string TopTopKey = "TopTop";
    [NonSerialized] public readonly string TopKey = "Top";
    [NonSerialized] public readonly string LeftKey = "Left";
    [NonSerialized] public readonly string RightKey = "Right";
    [NonSerialized] public readonly string BottomKey = "Bottom";
    [NonSerialized] public readonly string LeftUpKey = "LeftUpCorner";
    [NonSerialized] public readonly string RightUpKey = "RightUpCorner";
    [NonSerialized] public readonly string LeftDownKey = "LeftDownCorner";
    [NonSerialized] public readonly string RightDownKey = "RightDownCorner";
    
    [NonSerialized] public readonly string LeftDoorWayDown = "LeftDoorWayDown";
    [NonSerialized] public readonly string LeftDoorWayUp = "LeftDoorWayUp";
    [NonSerialized] public readonly string LeftDoorWayUpUp = "LeftDoorWayUpUp";
    [NonSerialized] public readonly string RightDoorWayDown = "RightDoorWayDown";
    [NonSerialized] public readonly string RightDoorWayUp = "RightDoorWayUp";
    [NonSerialized] public readonly string RightDoorWayUpUp = "RightDoorWayUpUp";

    private void OnValidate()
    {
        if (!WallTilesDictionary.ContainsKey(TopTopKey)) WallTilesDictionary.Add(TopTopKey, null);
        if (!WallTilesDictionary.ContainsKey(TopKey)) WallTilesDictionary.Add(TopKey, null);
        if (!WallTilesDictionary.ContainsKey(LeftKey)) WallTilesDictionary.Add(LeftKey, null);
        if (!WallTilesDictionary.ContainsKey(RightKey)) WallTilesDictionary.Add(RightKey, null);
        if (!WallTilesDictionary.ContainsKey(BottomKey)) WallTilesDictionary.Add(BottomKey, null);
        if (!WallTilesDictionary.ContainsKey(LeftUpKey)) WallTilesDictionary.Add(RightUpKey, null);
        if (!WallTilesDictionary.ContainsKey(RightUpKey)) WallTilesDictionary.Add(RightUpKey, null);
        if (!WallTilesDictionary.ContainsKey(LeftDownKey)) WallTilesDictionary.Add(LeftDownKey, null);
        if (!WallTilesDictionary.ContainsKey(RightDownKey)) WallTilesDictionary.Add(RightDownKey, null);

        if (!WallTilesDictionary.ContainsKey(LeftDoorWayUp)) WallTilesDictionary.Add(LeftDoorWayUp, null);
        if (!WallTilesDictionary.ContainsKey(LeftDoorWayDown)) WallTilesDictionary.Add(LeftDoorWayDown, null);
        if (!WallTilesDictionary.ContainsKey(RightDoorWayUp)) WallTilesDictionary.Add(RightDoorWayUp, null);
        if (!WallTilesDictionary.ContainsKey(RightDoorWayDown)) WallTilesDictionary.Add(RightDoorWayDown, null);
        if (!WallTilesDictionary.ContainsKey(LeftDoorWayUpUp)) WallTilesDictionary.Add(LeftDoorWayUpUp, null);
        if (!WallTilesDictionary.ContainsKey(RightDoorWayUpUp)) WallTilesDictionary.Add(RightDoorWayUpUp, null);
    }
}