using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Brawlley
{
    public enum AttackStatus { Ready, Cooldown }
    public enum PlayerViewDirection { Left, Right }

    public abstract class PlayerAttack : MonoBehaviour
    {
        #region Resources
        [Header("Player Attack Resources")]
        [SerializeField] protected Player player;
        //protected GameInputs GameInputs => player.GameInputs;
        #endregion

        #region Data
        [Header("Player Attack Data")]
        [SerializeField] protected float damage = 10f;
        [SerializeField] protected float knockbackForce = 5f;

        [Tooltip("The time in seconds between each attack.")]
        [SerializeField] protected float cooldown = 1f;
        [SerializeField] protected AttackStatus status = AttackStatus.Ready;
        [SerializeField] protected PlayerViewDirection playerViewDirection = PlayerViewDirection.Right;
        protected Vector2 direction;
        public Vector2 Direction { get => direction; set => direction = value; }
        #endregion

        #region MonoBehaviour Lifecycle Methods
        protected virtual void Start()
        {
            if (player == null)
                player = GetComponent<Player>();
        }
        #endregion

        #region Player Attack Methods
        public virtual void OnAttack(InputAction.CallbackContext context) {}

        IEnumerator CooldownCoroutine()
        {
            yield return new WaitForSeconds(cooldown);
            status = AttackStatus.Ready;
            Debug.Log("Cooldown finished");
        }

        protected void StartCooldown()
        {
            status = AttackStatus.Cooldown;
            StartCoroutine(CooldownCoroutine());
        }

        public void UpdateDirection(float x)
        {
            if (x < 0)
                playerViewDirection = PlayerViewDirection.Left;
            if (x > 0)
                playerViewDirection = PlayerViewDirection.Right;
        }
        #endregion
    }
}