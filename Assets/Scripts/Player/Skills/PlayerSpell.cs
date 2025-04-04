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
<<<<<<< HEAD
=======
        private Vector2 attackDirection;
        private Animator playerAnim;
>>>>>>> dev-sprites
        #endregion

        #region MonoBehaviour Lifecycle Methods
        protected override void Start()
        {
            base.Start();
            playerAnim = GetComponent<Animator>();
        }
        #endregion

        #region Player Spell Methods

        public override void OnAttack(InputAction.CallbackContext context)
        {
            if (status == AttackStatus.Ready)
            {
                if (direction == Vector2.zero)
                    direction.x = transform.localScale.x;

                playerAnim.SetTrigger("CastTrigger");

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
            }
        }
        #endregion
    }
}
