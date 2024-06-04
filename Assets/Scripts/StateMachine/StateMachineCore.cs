using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Mirror;
using UnityEngine;

public abstract class StateMachineCore : NetworkBehaviour
{
    [NonSerialized] public PlayerBehaviour Player;
    [Header("Core Data"), SerializeField] public Animator Animator;
    [SerializeField] public AnimationData AnimationData;

    public StateMachine StateMachine;
    public BaseState CurrentState => StateMachine?.CurrentState;

    public void SetupInstances(BaseState[] statesList)
    {
        StateMachine = new StateMachine();

        foreach (var state in statesList)
            state.SetCore(this);
    }

}
