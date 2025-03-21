using Brawlley.Attacks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Brawlley
{
    public class PlayerMelee : PlayerAttack
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created

        [SerializeField] bool canAttack = true;
        [SerializeField] Melee meleeAttack;
        [SerializeField] Riposte riposte;
        [SerializeField] float riposteForce = 1f;
        Animator playerAnim; 

        protected override void Start()
        {
            base.Start();
            playerAnim = GetComponent<Animator>();
            canAttack = true;
        }

        public override void OnAttack(InputAction.CallbackContext context)
        {
            if (context.started && canAttack)
            {
                meleeAttack.direction = direction;
                meleeAttack.ignoreTeam = player.Team;
                //meleeAttack.damage = damage;
                riposte.horizontalDirection = playerViewDirection == PlayerViewDirection.Right ? 1f : -1f;
                riposte.direction = direction;
                riposte.force = riposteForce;
                canAttack = false;
                playerAnim.SetTrigger("MeleeTrigger");
            }
            
        }
    }
}
