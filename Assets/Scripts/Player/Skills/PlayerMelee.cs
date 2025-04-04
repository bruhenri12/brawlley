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

        protected override void Start()
        {
            base.Start();
            playerAnim = GetComponent<Animator>();
        }

        public override void OnAttack(InputAction.CallbackContext context)
        {
            if (context.started && status == AttackStatus.Ready)
            {
                meleeAttack.direction = direction;
                meleeAttack.ignoreTeam = player.Team.name;
                meleeAttack.knockbackForce = knockbackForce;
                meleeAttack.damage = damage;
                riposte.horizontalDirection = playerViewDirection == PlayerViewDirection.Right ? 1f : -1f;
                riposte.direction = direction;
                riposte.force = riposteForce;
                playerAnim.SetTrigger("MeleeTrigger");
                StartCooldown();
            }
            
        }
    }
}
