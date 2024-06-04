using System.Collections;
using System.Collections.Generic;
using StateMachine.States;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/States/HitState")]
public class HitState : BaseState
{
    private Animator Animator => Core.Animator;
    
    public override void Enter()
    {
        base.Enter();
    }

    public override void Tick()
    {
        base.Tick();
    }
    
    public override void FixedTick()
    {
        base.FixedTick();

        if (Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !Animator.IsInTransition(0))
            IsComplete = true;
    }

    public override void Exit()
    {
        base.Exit();
    }
}
