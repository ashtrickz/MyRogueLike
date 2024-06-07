﻿using System;
using Mirror;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class EnvironmentAction : SerializedScriptableObject
{

    public virtual void OnInvoke(GameObject interactedObject) {}
    
}