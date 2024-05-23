using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(menuName = "Data/AnimationData")]
    public class AnimationData : SerializedScriptableObject
    {
        public Dictionary<AnimationState, AnimationClip> StateClips = new();

        public AnimationClip GetClipByState(AnimationState state)
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