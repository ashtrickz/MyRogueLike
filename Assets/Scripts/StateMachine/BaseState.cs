using System;
using System.Collections;
using System.Collections.Generic;
using DungeonGeneration;
using Sirenix.OdinInspector;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEngine;


public abstract class BaseState : MonoBehaviour
{
    public string AnimationName;
    
    protected StateMachineCore Core;
    protected float StartTime;

    public StateMachine Machine;
    public StateMachine ParentMachine;
    public BaseState State => Machine?.CurrentState;

    protected void Switch(BaseState newState, bool forceReset = false) => Machine.SwitchState(newState, forceReset);
    
    public bool IsComplete { get; protected set; }
    private Animator Animator => Core.Animator;
    public float ElapsedTime => Time.time - StartTime;

    public virtual void Enter() { Animator.Play(AnimationName); }

    public virtual void Tick() { }

    public virtual void FixedTick() { }

    public void TickBranch()
    {
        Tick();
        State?.TickBranch();
    }

    public void FixedTickBranch()
    {
        FixedTick();
        State?.FixedTickBranch();
    }

    public virtual void Exit() { }


    public void SetCore(StateMachineCore stateMachineCore)
    {
        Machine = new StateMachine();
        Core = stateMachineCore;
    }

    public void Initialise(StateMachine parent)
    {
        ParentMachine = parent;
        IsComplete = false;
        StartTime = UnityEngine.Time.time;
    }
}