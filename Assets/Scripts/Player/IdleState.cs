using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class IdleState : BaseState
{
    public IdleState(StateMachine stateMachine, PlayerBehaviour player, string animationHash) : base(stateMachine, player, animationHash)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Tick()
    {
        base.Tick();
        
        if (Player.PlayerControls.Player.Move.IsPressed()) StateMachine.Switch(StateMachine.RunState);;
    }


    public override void FixedTick()
    {
        base.FixedTick();
    }

    public override void Exit()
    {
        base.Exit();
    }
}
