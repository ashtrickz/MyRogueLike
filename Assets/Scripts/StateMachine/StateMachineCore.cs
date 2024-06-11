using System;
using Data;
using Mirror;
using StateMachine.Player;
using StateMachine.States;
using UnityEngine;

namespace StateMachine
{
    public abstract class StateMachineCore : NetworkBehaviour
    {
        [NonSerialized] public PlayerBehaviour Player;
        [NonSerialized] public EnemyBehaviour Enemy;
        [Header("Core Data"), SerializeField] public Animator Animator;
        [SerializeField] public AnimationData AnimationData;

        public StateMachine StateMachine;
        public BaseState CurrentState => StateMachine?.CurrentState;

        public void SetupInstances(BaseState[] statesList)
        {
            StateMachine = new StateMachine();

            foreach (var state in statesList)
                state.SetCore(this);
        }

    }
}
