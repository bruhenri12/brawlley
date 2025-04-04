using UnityEngine;
using UnityEngine.Events;

namespace Brawlley.Attacks
{
    public abstract class Attack : MonoBehaviour
    {
        #region Data
        [Header("Attack Data")]
        public float damage = 1f;
        public float knockbackForce = 1f;
        public string ignoreTeam;

        [Header("Events")]
        [SerializeField] UnityEvent onTriggerEnterEvent;
        protected Collider2D currentCollision;
        #endregion

        #region Collision Methods
        public void OnTriggerEnter2D(Collider2D collision)
        {
            currentCollision = collision;
            onTriggerEnterEvent.Invoke();
            currentCollision = null;
        }
        #endregion

        #region Attack Methods
        public virtual void OnPlayerCollisionDealDamage()
        {
            if (currentCollision != null && currentCollision.CompareTag("Player"))
            {
                if (currentCollision.TryGetComponent<Player>(out var collidedPlayer))
                {
                    if (collidedPlayer.Team.name == ignoreTeam)
                        return;

                    if (currentCollision.TryGetComponent<PlayerHealth>(out var playerHealth))
                        playerHealth.Damage += damage;

                    Destroy(gameObject);
                }

            }
        }
        #endregion
    }
}
