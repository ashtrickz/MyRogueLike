using System;
using System.Collections;
using System.Collections.Generic;
using AOT;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Generation/PropData")]
public class PropData : SerializedScriptableObject
{
    [SerializeField] public string stringId;
    [SerializeField] public Vector2[] colliderPoints;
    [SerializeField] public Sprite propSprite;
    [SerializeField] public Sprite shadowSprite;
    [SerializeField] public float durability;

    [SerializeField] public Vector2Int position;
    
    [SerializeField] public List<EnvironmentAction> OnDeathActions;
    
    private void OnValidate()
    {
        if (stringId == String.Empty) stringId = name;
    }
}