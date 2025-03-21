using UnityEngine;

namespace Brawlley.Attacks
{
    public class Riposte : MonoBehaviour
    {
        public float horizontalDirection;
        public Vector2 direction;
        public float force = 1f;
        public string ignoreTeam;
        void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Spell"))
            {  
                Spell spell = collision.GetComponent<Spell>();
                spell.ignoreTeam = ignoreTeam;
                spell.spellRigidbody.Sleep();
                Vector2 newDirection = direction * force;
                if (direction == Vector2.zero)
                {
                    Debug.Log("Rebateu paradodo");
                    newDirection.x = horizontalDirection * force;
                }
                spell.direction = newDirection;
                spell.ApplyGravity(direction.y > 0f ? spell.gravityScale : .2f);
                spell.ApplyForce();
            }
        }
    }
}
