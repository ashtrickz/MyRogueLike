using StateMachine.States;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/States/PatrolState")]
public class PatrolState : BaseState
{
    public Vector2 TimeBetweenPatroling = new Vector2(1, 3);
    public Vector2 PartolingDistance = new Vector2(5, 5);
    public float PatrolSpeedMultiplier = 0.75f;
    public int PointSelectTryCount = 5;

    private int _positionStuckRepeats = 20;
    private int _positionStuckCounter = 0;
    private float _timeBeforePartoling = 0;
    private bool _patrolingPaused = false;

    private Vector2 _patrolPoint;
    private Vector2 _cachedPosition;

    public EnemyBehaviour Enemy => Core.Enemy;

    public override void Enter()
    {
        base.Enter();

        StartWaiting();
    }

    private Vector2 SelectPatrolPoint()
    {
        var patrolPoint = new Vector2Int();
        var tries = PointSelectTryCount;

        while (tries >= 0)
        {
            patrolPoint = new Vector2Int(
                (int) (Enemy.transform.position.x + Random.Range(-PartolingDistance.x, PartolingDistance.x)),
                (int) (Enemy.transform.position.y + Random.Range(-PartolingDistance.y, PartolingDistance.y)));
            var hit = Physics2D.Raycast(Enemy.transform.position, patrolPoint,
                Vector2.Distance(Enemy.transform.position, patrolPoint));
            if (hit.collider.CompareTag("Obstacle") == false) break;
            tries--;
        }

        return new Vector2(patrolPoint.x + .5f, patrolPoint.y + .5f);
    }


    public override void Tick()
    {
        base.Tick();

        if (_patrolingPaused && ElapsedTime >= _timeBeforePartoling)
            StopWaiting();
    }

    public override void FixedTick()
    {
        base.FixedTick();

        CheckLookDirection();

        if (_patrolingPaused) return;

        CheckForStuck();

        if (Enemy.transform.position != (Vector3) _patrolPoint)
            Enemy.transform.position = Vector3.MoveTowards(Enemy.transform.position, _patrolPoint,
                Enemy.AnimationData.MoveSpeed * PatrolSpeedMultiplier);
        else StartWaiting();
    }

    private void CheckForStuck()
    {
        if (_cachedPosition == (Vector2) Enemy.transform.position)
        {
            _positionStuckCounter++;
            if (_positionStuckCounter >= _positionStuckRepeats)
                StartWaiting();
        }

        _cachedPosition = Enemy.transform.position;
    }

    private void StartWaiting()
    {
        _positionStuckCounter = 0;

        _timeBeforePartoling = ElapsedTime + Random.Range(TimeBetweenPatroling.x, TimeBetweenPatroling.y);
        _patrolingPaused = true;
        Switch(Enemy.IdleState, true);
    }

    private void StopWaiting()
    {
        _patrolingPaused = false;
        _patrolPoint = SelectPatrolPoint();
        Switch(Enemy.RunState);
    }

    private void CheckLookDirection()
    {
        var prevScale = Enemy.transform.localScale;
        var chasedX = _patrolPoint.x;
        var enemyX = Enemy.transform.position.x;
        if ((chasedX < enemyX && Enemy.transform.localScale.x > 0) ||
            (chasedX > enemyX && Enemy.transform.localScale.x < 0))
            Enemy.transform.localScale = new Vector3(-prevScale.x, prevScale.y, prevScale.z);
    }
}