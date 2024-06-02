using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseState
{
    public StateMachine StateMachine { get; private set; }

    public PlayerBehaviour Player;
    public string AnimationHash { get; private set; }

    public BaseState(StateMachine stateMachine, PlayerBehaviour player, string animationHash)
    {
        StateMachine = stateMachine;
        Player = player;
        AnimationHash = animationHash;
    }

    public bool IsComplete { get; protected set; }

    private float startTime;
    private float time => Time.time - startTime;

    public virtual void Enter()
    {
        Player.PlayStateAnimation(AnimationHash);
    }

    public virtual void Tick()
    {
    }

    public virtual void FixedTick()
    {
    }

    public virtual void Exit()
    {
    }
}