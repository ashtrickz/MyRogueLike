using System;
using System.Collections;
using System.Collections.Generic;
using StateMachine;
using StateMachine.Player;
using StateMachine.States;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyBehaviour : StateMachineCore
{
    [Space, Header("Required States")] public IdleState IdleState;
    public RunState RunState;
    public AttackState AttackState;
    public HitState HitState;
    public PatrolState PatrolState;
    public ChaseState ChaseState;

    [Space, SerializeField] private CircleCollider2D triggerCollider;
    [SerializeField] private float enemyDetectRadius = 2f;

    private void Start()
    {
        Enemy = this;
        
        IdleState = Instantiate(IdleState);
        RunState = Instantiate(RunState);
        AttackState = Instantiate(AttackState);
        HitState = Instantiate(HitState);
        PatrolState = Instantiate(PatrolState);
        ChaseState = Instantiate(ChaseState);

        triggerCollider.radius = enemyDetectRadius;
        triggerCollider.offset = new Vector2(enemyDetectRadius - .5f, 0);

        SetupInstances(new BaseState[] {IdleState, RunState, AttackState, HitState, PatrolState, ChaseState});
        StateMachine.SwitchState(PatrolState);
    }

    private void Update()
    {
        if (CurrentState.IsComplete)
        {
            Debug.Log($"{CurrentState} for {Enemy.name} is complete.");
            StateMachine.SwitchState(PatrolState);
        }

        CurrentState.TickBranch();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<PlayerBehaviour>();
        if (player == null) return;

        ChaseState.ChasedEntity = player.transform;
        StateMachine.SwitchState(ChaseState);
    }

    private void FixedUpdate()
    {
        CurrentState.FixedTickBranch();
    }
}