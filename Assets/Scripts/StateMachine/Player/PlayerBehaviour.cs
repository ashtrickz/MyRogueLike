using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Data;
using Helpers;
using Mirror;
using Sirenix.OdinInspector;
using Sirenix.Reflection.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
using AnimationState = Data.AnimationData.AnimationState;

public class PlayerBehaviour : StateMachineCore
{
    public IdleState IdleState;
    public RunState RunState;
    public AttackState AttackState;
    
    public PlayerControls PlayerControls { get; private set; }

    private void Start()
    {
        if (!isLocalPlayer) return;

        Player = this;
        
        var camera = FindObjectOfType<CinemachineVirtualCamera>(); //TODO get camera from some kind of singleton or else
        camera.Follow = transform;

        PlayerControls = new();
        PlayerControls.Enable();

        SetupInstances();
        StateMachine.SwitchState(IdleState);
    }

    [ClientCallback]
    private void Update()
    {
        if (!isLocalPlayer) return;

        CheckAttack();
        
        if (CurrentState.IsComplete)
        {
            StateMachine.SwitchState(IdleState);
        }
        
        CurrentState.TickBranch();
    }

    private void CheckAttack()
    {
        if (!AttackPressed) return;
        StateMachine.SwitchState(AttackState);
        CurrentState.Machine.SwitchState(IdleState);
    }

    [ClientCallback]
    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;

        CurrentState.FixedTickBranch();
        
        if (MovePressed && CurrentState.IsComplete) StateMachine.SwitchState(RunState);
    }

    [ClientCallback]
    public void PlayStateAnimation(string animationHash) => Animator.Play(animationHash);
    public bool MovePressed => PlayerControls.Player.Move.IsPressed();
    
    public bool AttackPressed => PlayerControls.Player.MainAttack.IsPressed()
                                    || PlayerControls.Player.SecondaryAttack.IsPressed()
                                    || PlayerControls.Player.MagicAttack.IsPressed();


    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying || StateMachine == null) return;
        List<BaseState> states = StateMachine.GetActiveStateBranch();
        
        if (states == null || states.Count == 0) return;
        
        Helper.DrawString("Active States: " + string.Join(" > ", states),
            new Vector3(transform.position.x + .02f, transform.position.y + 1.18f, transform.position.z), Color.black);
        Helper.DrawString("Active States: " + string.Join(" > ", states),
            new Vector3(transform.position.x, transform.position.y + 1.2f, transform.position.z), Color.red);
#endif
    }


}