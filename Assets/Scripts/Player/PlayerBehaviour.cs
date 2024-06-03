using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Data;
using Helpers;
using Mirror;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
using AnimationState = Data.AnimationData.AnimationState;

public class PlayerBehaviour : NetworkBehaviour
{
    [SerializeField, InlineEditor] private AnimationData animationData;
    [SerializeField] private Animator animator;

    [Space, SerializeField] private Transform mainWeaponContainer;
    [SerializeField] private Transform secondaryWeaponContainer;
    [SerializeField] private Transform magicWeaponContainer;
    
    private string _currentState => _stateMachine != null ? _stateMachine.State.ToString() : "Player's Current State";

    public AnimationData AnimationData => animationData;
    
    public PlayerControls PlayerControls { get; private set; }

    private StateMachine _stateMachine;

    [SerializeField] private WeaponBase currentWeapon;
    
    public WeaponBase CurrentWeapon => currentWeapon;


    private void Start()
    {
        if (!isLocalPlayer) return;

        var camera = FindObjectOfType<CinemachineVirtualCamera>(); //TODO get camera from some kind of singleton or else
        camera.Follow = transform;

        PlayerControls = new();
        PlayerControls.Enable();
        
        _stateMachine = new StateMachine(this, animationData);
    }
    
    [ClientCallback]
    private void Update()
    {
        if (!isLocalPlayer) return;
        
        _stateMachine.State.Tick();
    }
    
    [ClientCallback]
    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        
        _stateMachine.State.FixedTick();
    }
    
    [ClientCallback]
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
