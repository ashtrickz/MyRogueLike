using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Mirror.Examples.CCU;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using AnimationState = Data.AnimationData.AnimationState;
using Random = UnityEngine.Random;

public class StateMachine
{
    public BaseState CurrentState { get; protected set; }
    
    public void SwitchState(BaseState state, bool forceReset = false)
    {
        if (CurrentState == state && !forceReset) return;

        CurrentState?.Exit();
        CurrentState = state;
        CurrentState.Initialise();
        CurrentState.Enter();
    }

    // public List<BaseState> GetActiveStateBranch(List<BaseState> list = null)
    // {
    //     list ??= new();
    //
    //     if (CurrentState == null) return list;
    //
    //     list.Add(CurrentState);
    //     return CurrentState.SubStateMachine.GetActiveStateBranch(list);
    // }
}