using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Mirror;
using UnityEngine;

public abstract class StateMachineCore : NetworkBehaviour
{
    [NonSerialized] public PlayerBehaviour Player;
    [SerializeField] public AnimationData AnimationData;
    [SerializeField] public Animator Animator;

    public StateMachine StateMachine;

    public void SetupInstances()
    {
        StateMachine = new StateMachine();
        
        BaseState[] statesList = GetComponentsInChildren<BaseState>();
        foreach (var state in statesList)
            state.SetCore(this);
    }

}
