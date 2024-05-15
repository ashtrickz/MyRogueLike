using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Generation/PropData")]
public class PropData : SerializedScriptableObject
{
    [SerializeField] public string stringId;
    [SerializeField] public Vector2[] colliderPoints;
    [SerializeField] public Rigidbody2D rigidbodyData;
    [SerializeField] public Sprite propSprite;
    [SerializeField] public Sprite shadowSprite;
    [SerializeField] public float durability;

    private void OnValidate()
    {
        if (stringId == String.Empty) stringId = name;
    }
}
