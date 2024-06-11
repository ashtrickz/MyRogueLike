using StateMachine.Player;
using StateMachine.States;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/States/ChaseState")]
public class ChaseState : BaseState
{

    public float BreakAwayDistance = 5f;
    public EnemyBehaviour Enemy => Core.Enemy;
    
    public Transform ChasedEntity;

    public override void Enter()
    {
        base.Enter();

        Debug.Log($"{Enemy.name} started chasing.");
        
        Switch(Enemy.RunState);
    }
    
    public override void Tick()
    {
        base.Tick();
    }

    public override void FixedTick()
    {
        base.FixedTick();

        CheckLookDirection();
        
        if (Enemy.transform.position != (Vector3) ChasedEntity.position && Vector3.Distance(Enemy.transform.position, ChasedEntity.transform.position) < BreakAwayDistance)
            Enemy.transform.position = Vector3.MoveTowards(Enemy.transform.position, ChasedEntity.position,
                Enemy.AnimationData.MoveSpeed);
        else IsComplete = true;

    }

    private void CheckLookDirection()
    {
        var prevScale = Enemy.transform.localScale;
        var chasedX = ChasedEntity.transform.position.x;
        var enemyX = Enemy.transform.position.x;
        if ((chasedX < enemyX && Enemy.transform.localScale.x > 0) || (chasedX > enemyX && Enemy.transform.localScale.x < 0)) 
            Enemy.transform.localScale = new Vector3(-prevScale.x, prevScale.y, prevScale.z);
    }
}