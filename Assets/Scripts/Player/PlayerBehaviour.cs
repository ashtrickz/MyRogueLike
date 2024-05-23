using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;
using AnimationState = Data.AnimationData.AnimationState;

public class PlayerBehaviour : SerializedMonoBehaviour
{
    [SerializeField, InlineEditor] private AnimationData animationData;
    [SerializeField] private Animator animator;
    
    private BaseState currentState;

    private StateMachine _stateMachine;
    
    private IdleState _idleState;
    private RunState _runState;
    private AttackState _attackState;
    
    private void Start()
    {
        _idleState = new(animationData.GetClipByState(AnimationState.Idle));
        _runState = new(animationData.GetClipByState(AnimationState.Run));
        _attackState = new(animationData.GetClipByState(AnimationState.Attack));

        _stateMachine = new StateMachine(this);
        
        currentState = _idleState;
    }

    public void PlayStateAnimation(AnimationClip stateClip)
    {
        animator.Play(stateClip.name);
    }
    
}
