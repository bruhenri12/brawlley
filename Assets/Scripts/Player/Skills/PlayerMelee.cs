using Brawlley.Attacks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Brawlley
{
    public class PlayerMelee : PlayerAttack
    {
        [SerializeField] Melee meleeAttack;
        [SerializeField] Riposte riposte;
        [SerializeField] float riposteForce = 1f;
        Animator playerAnim;
        private bool isHoldingMelee = false;

        protected override void Start()
        {
            base.Start();
            playerAnim = GetComponent<Animator>();
            
            meleeAttack.ignoreTeam = player.Team.name;
            riposte.ignoreTeam = player.Team.name;
        }
        public void PrepareAttack(InputAction.CallbackContext context)
        {
            if (!isHoldingMelee)
            {
                playerAnim.SetTrigger("MeleeStartTrigger");
                isHoldingMelee = true;
            }

        }
        public override void OnAttack(InputAction.CallbackContext context)
        {
            if (context.canceled)
            {
                playerAnim.SetBool("MeleeAttack", true);
                if (status == AttackStatus.Ready)
                {
                    meleeAttack.direction = direction;
                    meleeAttack.knockbackForce = knockbackForce;
                    meleeAttack.damage = damage;
                    riposte.horizontalDirection = playerViewDirection == PlayerViewDirection.Right ? 1f : -1f;
                    riposte.direction = direction;
                    riposte.force = riposteForce;

                    StartCooldown();
                }
            } 
            
        }

        public void OnRemoteAttack()
        {
			playerAnim.SetBool("MeleeAttack", true);
			if (status == AttackStatus.Ready)
			{
				meleeAttack.direction = direction;
				meleeAttack.knockbackForce = knockbackForce;
				meleeAttack.damage = damage;
				riposte.horizontalDirection = playerViewDirection == PlayerViewDirection.Right ? 1f : -1f;
				riposte.direction = direction;
				riposte.force = riposteForce;

				StartCooldown();
			}
            
        }

        public void ResetMeleeAttack()
        {
            playerAnim.SetBool("MeleeAttack", false);
            isHoldingMelee = false;
        }

    }
}
