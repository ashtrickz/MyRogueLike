using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using UnityEngine;
using UnityEngine.InputSystem;
using AnimationState = Data.AnimationData.AnimationState;

public class StateMachine
{
    public PlayerBehaviour Player { get; protected set; }

    public AnimationData AnimationData { get; protected set; }
    
    public BaseState State { get; protected set; }

    public IdleState IdleState { get; protected set; }
    public RunState RunState { get; protected set; }
    public AttackState AttackState { get; protected set; }
    
    public StateMachine(PlayerBehaviour player, AnimationData animationData)
    {
        Player = player;
        AnimationData = animationData;
        
        Init();
    }

    private void Init()
    {
        IdleState = new(this, Player, AnimationData.GetAnimationHashByState(AnimationState.Idle));
        RunState = new(this, Player, AnimationData.GetAnimationHashByState(AnimationState.Run));
        AttackState = new(this, Player, AnimationData.GetAnimationHashByState(AnimationState.Attack));

        Switch(IdleState);
    }
    
    public void Switch(BaseState state, bool forceReset = false)
    {
        if (State == state && !forceReset) return;
        
        State?.Exit();
        State = state;
        State.Enter();
    }
    
}
