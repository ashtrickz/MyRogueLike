using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : BaseState
{
    public AttackState(StateMachine stateMachine, PlayerBehaviour player, string animationHash) : base(stateMachine, player, animationHash)
    {
    }

    private WeaponBase Weapon => Player.CurrentWeapon;

    private float _attackTime = 0;
    
    public override void Enter()
    {
        if (Player.CurrentWeapon == null) 
            StateMachine.Switch(StateMachine.IdleState);
        
        base.Enter();

        _attackTime = 0;
    }

    public override void Tick()
    {
        base.Tick();
        Weapon.WeaponTick();

        switch (Weapon.Type)
        {
            case WeaponBase.WeaponType.Main:
                Player.PlayerControls.Player.MainAttack.performed += attack => Weapon.Attack();
                break;
            case WeaponBase.WeaponType.Secondary:
                Player.PlayerControls.Player.SecondaryAttack.performed += attack => Weapon.Attack();
                break;
            case WeaponBase.WeaponType.Magic:
                Player.PlayerControls.Player.MagicAttack.performed += attack => Weapon.Attack();
                break;
            
        }

    }

    public override void FixedTick()
    {
        base.FixedTick();

        
        if (_attackTime < Weapon.AttackSpeed)
            _attackTime += Time.deltaTime;
        else
        {
            StateMachine.Switch(StateMachine.IdleState);
        }

    }

    public override void Exit()
    {
        base.Exit();
    }
}
