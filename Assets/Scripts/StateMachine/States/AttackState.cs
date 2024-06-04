using UnityEngine;

namespace StateMachine.States
{
    [CreateAssetMenu(menuName = "Data/States/AttackState")]
    public class AttackState : BaseState
    {

        // private WeaponBase Weapon => Player.CurrentWeapon;
    
        public override void Enter()
        {
            base.Enter();

            Machine.SwitchState(Core.Player.IdleState);
        
        }

        public override void Tick()
        {
            base.Tick();

        }

        public override void FixedTick()
        {
            base.FixedTick();

            if (Core.Player.MovePressed) Machine.SwitchState(Core.Player.RunState);
        
            if (ElapsedTime > 1f)
                IsComplete = true;

        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
