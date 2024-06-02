using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Helpers;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
using AnimationState = Data.AnimationData.AnimationState;

public class PlayerBehaviour : SerializedMonoBehaviour
{
    [SerializeField, InlineEditor] private AnimationData animationData;
    [SerializeField] private Animator animator;
    
    private string _currentState => _stateMachine != null ? _stateMachine.State.ToString() : "Player's Current State";
    
    public PlayerControls PlayerControls { get; private set; }

    private StateMachine _stateMachine;

    private void Start()
    {
        PlayerControls = new();
        PlayerControls.Enable();
        
        _stateMachine = new StateMachine(this, animationData);
    }
    
    private void Update()
    {
        _stateMachine.State.Tick();
    }

    private void FixedUpdate()
    {
        _stateMachine.State.FixedTick();
    }
    
    public void PlayStateAnimation(string animationHash)
    {
        animator.Play(animationHash);
    }
    
#if UNITY_EDITOR    
    void OnDrawGizmos() 
    {
        Helper.DrawString(_currentState, new Vector3(transform.position.x +.02f, transform.position.y + 1.18f, transform.position.z), Color.black);
        Helper.DrawString(_currentState, new Vector3(transform.position.x, transform.position.y + 1.2f, transform.position.z), Color.red);
    }

#endif
}
