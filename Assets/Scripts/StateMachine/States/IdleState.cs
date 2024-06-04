using System;
using System.Collections;
using System.Collections.Generic;
using StateMachine.States;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Data/States/IdleState")]
public class IdleState : BaseState
{
    public override void Enter()
    {
        base.Enter();
        IsComplete = true;
    }

    public override void Tick()
    {
        base.Tick();

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
