using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FistsWeapon : WeaponBase
{
    public override void Attack()
    {
        base.Attack();

        Collider2D[] entities = Physics2D.OverlapCircleAll(transform.position, 1);
        List<IDamageable> damageables = new();
        foreach (var entity in entities)
        {
            var damageable = entity.gameObject.GetComponent<IDamageable>();
            if (damageable == null) continue;
            damageables.Add(damageable);
        }

        foreach (var damageable in damageables)
        {
            // Player.RemoveObjectAuthority(damageable.Identity);
            // Player.AddObjectAuthority(damageable.Identity);
            damageable.OnHitTaken(Damage);
        }
    }
}
