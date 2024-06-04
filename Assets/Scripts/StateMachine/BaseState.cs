using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEngine;


public abstract class BaseState : MonoBehaviour
{
    public string AnimationName;
    
    protected StateMachineCore Core;
    protected float StartTime;

    public bool IsComplete { get; protected set; }
    
    private StateMachine Machine => Core.StateMachine;
    private Animator Animator => Core.Animator;
    public BaseState State => Machine.CurrentState;
    public float Time => UnityEngine.Time.time - StartTime;

    public virtual void Enter() { Animator.Play(AnimationName); }

    public virtual void Tick() { }

    public virtual void FixedTick() { }

    public virtual void Exit() { }

    public void SetCore(StateMachineCore stateMachineCore) => Core = stateMachineCore;

    public void Initialise()
    {
        IsComplete = false;
        StartTime = UnityEngine.Time.time;
    }
}