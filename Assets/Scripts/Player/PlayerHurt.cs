using Brawlley;
using Brawlley.Attacks;
using UnityEngine;
using UnityEngine.UI; // Ensure you have this for Image

public class PlayerHurt : MonoBehaviour
{
    Player player;
    private PlayerHealth playerHealth;
    private Rigidbody2D playerRb;
    private GameManager gameManager;
    [SerializeField] BoxCollider2D playerHurtbox;

    private void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>();
        gameManager = FindAnyObjectByType<GameManager>(); // Ensure this is correct for accessing the GameManager
        player = GetComponent<Player>();
    }

    public void GetHit(Vector2 direction)
    {
        playerHealth.Damage += 10; // Increase damage
        Debug.Log("Player Health: " + playerHealth.Damage);
        gameManager.HandlePlayerDamage(this.gameObject, playerHealth.Damage);

        Image healthCircleImage = gameManager.GetHealthCircleImage(this.gameObject);
        if (healthCircleImage != null)
        {
            gameManager.HandlePlayerDamage(this.gameObject, playerHealth.Damage);
        }


        playerRb.linearVelocity = direction * playerHealth.Damage;
    }

    // OnTriggerEnter2D � usado para lidar a colis�o entre o player e o proj�til (trigger)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("WorldBounds")) { return; }

        if (collision.CompareTag("Spell"))
        {
            Spell spell = collision.GetComponent<Spell>();
            if (spell.ignoreTeam == player.Team) { return; }
            Debug.Log("Apanhou Spell");
            GetHit(spell.direction);
            Destroy(collision.gameObject);
        }
        else if (collision.CompareTag("Melee"))
        {
            Melee meleeAttack = collision.GetComponent<Melee>();
            if (meleeAttack.ignoreTeam == player.Team) { return; }
            Debug.Log("Apanhou Melee");
            if (meleeAttack.direction == Vector2.zero) { GetHit(new(meleeAttack.transform.localScale.x, 0)); return; }
            GetHit(meleeAttack.direction);
        }
        if (playerHurtbox.IsTouching(collision))
        {
        }
        
    }

    private void Respawn()
    {
        playerHealth.ResetHealth();
        playerRb.linearVelocity = Vector2.zero;
        transform.position = new Vector3(0, 3, 0);
        gameManager.HandlePlayerDamage(this.gameObject, playerHealth.Damage);
    }

    public void Die()
    {
        playerHealth.Lives--;
        gameManager.HandlePlayerDeath(this.gameObject, playerHealth.Lives);        
        if (playerHealth.Lives > 0) { Respawn(); }
    }
}
