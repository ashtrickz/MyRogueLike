using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public interface IDamageable
{
    public float HealthPoints { get; set; }

    public Action OnHitTakenAction { get; set; }
    public Action<GameObject> OnDeathAction { get; set; }
    public NetworkIdentity Identity { get; set; }
    public GameObject InteractionGameObject { get; set; }

    public void OnHitTaken(float damage)
    {
        OnHitTakenAction?.Invoke();
        
        HealthPoints = damage > HealthPoints ? 0 : HealthPoints -= damage;

        if (HealthPoints <= 0) OnDeathAction?.Invoke(InteractionGameObject);
    }

}