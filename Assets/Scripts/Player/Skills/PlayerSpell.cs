using Brawlley.Attacks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Brawlley
{
    public class PlayerSpell : PlayerAttack
    {
        #region Resources
        [Header("Player Spell Resources")]
        [SerializeField] GameObject spellPrefab;
        [SerializeField] Transform spellSpawnPoint;
        #endregion

        #region Data
        [Header("Player Spell Data")]
        [SerializeField] float travelSpeed = 1f;
        [SerializeField] float verticalCastGravity = 0.5f;
        #endregion

        private Animator playerAnim;
        private bool isHoldingCast = false;
        [SerializeField] GameObject orb;

        #region MonoBehaviour Lifecycle Methods
        protected override void Start()
        {
            base.Start();
            playerAnim = GetComponent<Animator>();
        }
        #endregion

        #region Player Spell Methods

        public void PrepareAttack(InputAction.CallbackContext context)
        {
            if (!isHoldingCast)
            {
                playerAnim.SetTrigger("CastTrigger");
                isHoldingCast = true;
            }
            
        }

        public override void OnAttack(InputAction.CallbackContext context)
        {
            if (status == AttackStatus.Ready)
            {
                playerAnim.SetFloat("AimingDirY", direction.y);
                playerAnim.SetBool("CastAttack", true);
            }
            else
            {
                playerAnim.SetTrigger("CancelCastTrigger");
            }
        }

        public void OnRemoteAttack()
        {
            if (status == AttackStatus.Ready)
            {
                playerAnim.SetFloat("AimingDirY", direction.y);
                playerAnim.SetBool("CastAttack", true);
            }
            else
            {
                playerAnim.SetTrigger("CancelCastTrigger");
            }
        }


        public void CastSpell()
        {
            orb.GetComponent<Orb>().Disable(cooldown);

            if (direction == Vector2.zero)
                direction.x = transform.localScale.x;

            GameObject spellObject = Instantiate(spellPrefab, spellSpawnPoint.position, Quaternion.identity);

            Spell spell = spellObject.GetComponent<Spell>();
            spell.damage = damage;
            spell.knockbackForce = knockbackForce;
            spell.ignoreTeam = player.Team.name;
            spell.speed = travelSpeed;
            spell.direction = direction;
            spell.gravityScale = verticalCastGravity;
            spell.ApplyGravity(direction.y > 0f ? verticalCastGravity : 0f);
            spell.ApplyForce();

            StartCooldown();
            

            playerAnim.SetBool("CastAttack", false);
            isHoldingCast = false;
        }

        #endregion
    }
}