using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class RunState : BaseState
{
    private Vector2 _playerMoveDirection;

    private Rigidbody2D _playerRb;
    
    public RunState(StateMachine stateMachine, PlayerBehaviour player, string animationHash) : base(stateMachine, player, animationHash)
    {
    }

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
        
        _playerMoveDirection = Player.PlayerControls.Player.Move.ReadValue<Vector2>();
        if (_playerMoveDirection == Vector2.zero) StateMachine.Switch(StateMachine.IdleState);
        else MovePlayer();
    }

    private void MovePlayer()
    {
        Player.transform.position += (Vector3)(_playerMoveDirection * StateMachine.AnimationData.MoveSpeed);
        var prevScale = Player.transform.localScale;
        if ((_playerMoveDirection.normalized.x < 0 && Player.transform.localScale.x > 0) || (_playerMoveDirection.normalized.x > 0 && Player.transform.localScale.x < 0)) 
            Player.transform.localScale = new Vector3(-prevScale.x, prevScale.y, prevScale.z);

    }

    public override void Exit()
    {
        base.Exit();
    }
}
