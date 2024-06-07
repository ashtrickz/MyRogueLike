using System;
using System.Collections.Generic;
using Cinemachine;
using Helpers;
using Mirror;
using StateMachine.States;
using Unity.VisualScripting;
using UnityEngine;
using Random = System.Random;

namespace StateMachine.Player
{
    public class PlayerBehaviour : StateMachineCore
    {
        [Space, Header("Required States")] public IdleState IdleState;
        public RunState RunState;
        public AttackState AttackState;
        public HitState HitState;

        [Space, Header("References")] public Animator WeaponAnimator;
        public Action OnPlayerHit;
        [SerializeField] private SpriteRenderer playerSprite;

        public PlayerControls PlayerControls { get; private set; }

        #region Mono Logic

        private void Start()
        {
            if (!isLocalPlayer) return;

            Player = this;

            //TODO get camera from some kind of singleton or else
            var camera = FindObjectOfType<CinemachineVirtualCamera>(); 
            camera.Follow = transform;

            PlayerControls = new();
            PlayerControls.Enable();

            OnPlayerHit += OnPlayerHitCallback;

            SetupInstances(new[] {(BaseState) IdleState, RunState, AttackState, HitState});
            StateMachine.SwitchState(IdleState);

            // Networking

            ChangePlayersColorCmd();
            GenerateDungeonCmd();
        }

        private void Update()
        {
            if (!isLocalPlayer) return;

            CheckAttack();

            if (CurrentState.IsComplete)
            {
                StateMachine.SwitchState(IdleState);
            }

            CurrentState.TickBranch();
        }

        private void FixedUpdate()
        {
            if (!isLocalPlayer) return;

            CurrentState.FixedTickBranch();

            if (MovePressed && CurrentState.IsComplete) StateMachine.SwitchState(RunState);
        }

        public void PlayStateAnimation(string animationHash) => Animator.Play(animationHash);

        private void CheckAttack()
        {
            if (!AttackPressed) return;
            StateMachine.SwitchState(AttackState);
            CurrentState.Machine.SwitchState(IdleState);
        }

        public void OnPlayerHitCallback()
        {
            StateMachine.SwitchState(HitState);
        }

        #endregion

        #region Network Logic

        // Dungeon Generation
        
        [Command]
        private void GenerateDungeonCmd()
        {
            NetworkDungeonManager.Instance.GenerateDungeonClientRpc();
        }

        // Authority

        [Command]
        public void AddObjectAuthorityCmd(NetworkIdentity netObject) =>
            netObject.AssignClientAuthority(connectionToClient);

        [Command]
        public void RemoveObjectAuthorityCmd(NetworkIdentity netObject) =>
            netObject.RemoveClientAuthority();

        // Change Players Stats
        
        [Command]
        private void ChangePlayersColorCmd()
        {
            foreach (var connection in NetworkServer.connections)
            {
                var player = connection.Value.identity.GetComponent<PlayerBehaviour>();
                var randomColor = UnityEngine.Random.ColorHSV() * 4;

                ChangePlayersColorRpc(player, randomColor);
            }
        }

        [ClientRpc]
        private void ChangePlayersColorRpc(PlayerBehaviour player, Color color)
        {
            player.playerSprite.color = color;
        }

        #endregion

        public bool MovePressed => PlayerControls.Player.Move.IsPressed();

        public bool AttackPressed => PlayerControls.Player.MainAttack.IsPressed()
                                     || PlayerControls.Player.SecondaryAttack.IsPressed()
                                     || PlayerControls.Player.MagicAttack.IsPressed();

        public SpriteRenderer PlayerSprite => playerSprite;


        void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying || StateMachine == null) return;
            List<BaseState> states = StateMachine.GetActiveStateBranch();

            if (states == null || states.Count == 0) return;

            Helper.DrawString("Active States: " + string.Join(" > ", states).Replace("Player_", ""),
                new Vector3(transform.position.x + .02f, transform.position.y + 1.18f, transform.position.z),
                Color.black);
            Helper.DrawString("Active States: " + string.Join(" > ", states).Replace("Player_", ""),
                new Vector3(transform.position.x, transform.position.y + 1.2f, transform.position.z), Color.red);
#endif
        }
    }
}