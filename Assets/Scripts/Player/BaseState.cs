using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseState
{

    public AnimationClip StateClip { get; private set; }

    public BaseState(AnimationClip clip)
    {
        StateClip = clip;
    }
    
    public bool IsComplete { get; protected set; }

    private float startTime;
    private float time => Time.time - startTime;

    public AnimationClip Clip { get; protected set; }
    
    public virtual void Enter() {}
    public virtual void Tick() {}
    public virtual void FixedTick() {}
    public virtual void Exit() {}
}
