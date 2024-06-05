using StateMachine.Player;
using UnityEngine;

namespace StateMachine.States
{
    [CreateAssetMenu(menuName = "Data/States/AttackState")]
    public class AttackState : BaseState
    {

        private PlayerBehaviour Player => Core.Player;
        private Animator Animator => Player.WeaponAnimator;
    
        public override void Enter()
        {
            Animator.Play(AnimationName); //TODO Change Animation Name on Used Weapon Animation Name
            Machine.SwitchState(Core.Player.IdleState);
        }

        public override void Tick()
        {
            base.Tick();

        }

        public override void FixedTick()
        {
            base.FixedTick();

            if (State.IsComplete)
            {
                Machine.SwitchState(Player.IdleState);
            }
            
            if (Player.MovePressed) Machine.SwitchState(Player.RunState);
            if (ElapsedTime > 1f)
                IsComplete = true;

        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
