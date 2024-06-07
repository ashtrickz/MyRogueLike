using System.Collections;
using System.Collections.Generic;
using Mirror;
using StateMachine.Player;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public abstract class WeaponBase : NetworkBehaviour
{

    public PlayerBehaviour Player;
    
    public float Damage;
    public float AttackSpeed;
    public float CooldDown;

    public WeaponType Type;

    public virtual void Init(PlayerBehaviour player)
    {
        Player = player;
    }
    
    public virtual void Attack() { }

    public virtual void WeaponTick() { } 
    
    public virtual void WeaponFixedTick() { }

    public enum WeaponType
    {
        Main,
        Secondary,
        Magic
    }
    
}
