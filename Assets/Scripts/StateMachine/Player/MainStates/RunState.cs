using System.Collections;
using System.Collections.Generic;
using Data;
using Sirenix.OdinInspector;
using UnityEngine;

public class RunState : BaseState
{
    private Vector2 _playerMoveDirection;

    private PlayerBehaviour Player => Core.Player;
    
    public override void Enter()
    {
        base.Enter();
    }

    public override void Tick()
    {
        base.Tick();
        
        //if (Player.AttackPressed) SwitchSubState(StateMachine.AttackSubState);
    }

    public override void FixedTick()
    {
        base.FixedTick();
        
        if (Player != null)
        {
            _playerMoveDirection = Player.PlayerControls.Player.Move.ReadValue<Vector2>();
            if (_playerMoveDirection == Vector2.zero) IsComplete = true;
            else MovePlayer();
        }
    }

    private void MovePlayer()
    {
        Vector3 moveInput = _playerMoveDirection * Core.AnimationData.MoveSpeed;
        Player.transform.position += _playerMoveDirection.x != 0 && _playerMoveDirection.y != 0 
            ? moveInput * Player.AnimationData.DiagonalMovementMultiplier
            : moveInput;
        
        var prevScale = Player.transform.localScale;
        if ((_playerMoveDirection.normalized.x < 0 && Player.transform.localScale.x > 0) || (_playerMoveDirection.normalized.x > 0 && Player.transform.localScale.x < 0)) 
            Player.transform.localScale = new Vector3(-prevScale.x, prevScale.y, prevScale.z);

    }

    public override void Exit()
    {
        base.Exit();
    }
}
