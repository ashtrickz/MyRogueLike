using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public PlayerBehaviour Player { get; protected set; }

    public BaseState State { get; protected set; }

    public StateMachine(PlayerBehaviour player)
    {
        Player = player;
    }

    public void Switch(BaseState state, bool forceReset = false)
    {
        if (State == state && !forceReset) return;
        
        State?.Exit();
        State = state;
        State.Enter();
    }

    private void Update()
    {
        State.Tick();
    }

    private void FixedUpdate()
    {
        State.FixedTick();
    }
}
