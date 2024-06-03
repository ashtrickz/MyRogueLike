using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(menuName = "Data/AnimationData")]
    public class AnimationData : SerializedScriptableObject
    {
        public Dictionary<AnimationState, string> StateClips = new();

        public float MoveSpeed = 5f;
        public float DiagonalMovementMultiplier = .75f;
        
        public string GetAnimationHashByState(AnimationState state)
        {
            return !StateClips.ContainsKey(state) ? null : StateClips[state];
        }

        public enum AnimationState
        {
            Idle,
            Run,
            Attack
        }
    }
}