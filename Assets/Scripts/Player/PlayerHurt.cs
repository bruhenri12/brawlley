using System;
using Brawlley;
using Brawlley.Attacks;
using UnityEngine;
using UnityEngine.UI; // Ensure you have this for Image

public class PlayerHurt : MonoBehaviour
{
    Player player;
    private PlayerHealth playerHealth;
    PlayerDash playerDash;
    private PlayerJuice juice;
    private Rigidbody2D playerRb;
    private GameManager gameManager;
    [SerializeField] BoxCollider2D playerHurtbox;

    private void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>();
        playerDash = GetComponent<PlayerDash>();
        gameManager = FindAnyObjectByType<GameManager>(); // Ensure this is correct for accessing the GameManager
        player = GetComponent<Player>();
        juice = GetComponent<PlayerJuice>();
    }

    public void GetHit(Vector2 direction, float damage = 10, float knockbackForce = 1f)
    {
        playerHealth.Damage += damage; // Increase damage
        Debug.Log("Damage taken: " + damage);
        gameManager.HandlePlayerDamage(player, playerHealth.Damage);

        playerRb.linearVelocity = playerHealth.Damage * direction;
    }

    // OnTriggerEnter2D � usado para lidar a colis�o entre o player e o proj�til (trigger)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("WorldBounds")) { return; }
        if (gameManager.isPreparing) { return; } // Ignore if in preparation phase
        if (playerDash.IsDashing) { return; } // Ignore if dashing

        if (collision.CompareTag("Spell"))
        {
            //Chamar de Destruir os Spell no Player
            juice.SpellHurtJuice(collision.transform.position);
            Spell spell = collision.GetComponent<Spell>();
            if (spell.ignoreTeam == player.Team.name) { return; }
            Debug.Log("Apanhou Spell");
            GetHit(spell.direction, spell.damage, spell.knockbackForce);
            Destroy(collision.gameObject);
        }
        else if (collision.CompareTag("Melee"))
        {
            //Chamar o Ofeito de Colisão Player Player
            
            Melee meleeAttack = collision.GetComponent<Melee>();
            juice.MeleeHurtJuice(meleeAttack.orbTransform.position);
            if (meleeAttack.ignoreTeam == player.Team.name) { return; }
            Debug.Log("Apanhou Melee");
            GetHit((playerRb.position - (Vector2)collision.transform.position).normalized, meleeAttack.damage, meleeAttack.knockbackForce);
        }
        if (playerHurtbox.IsTouching(collision))
        {
        }
        
    }

    private void Respawn()
    {
        playerRb.linearVelocity = Vector2.zero;
        if (gameManager.isPreparing)
        {
            transform.position = player.spawnPoint.position;
            return; // Ignore if in preparation phase
        }
        Vector3 spawnPoint = gameManager.GetRandomSpawnPoint().position;
        if (gameManager.map == Map.Volley)
        {
            spawnPoint = player.spawnPoint.position;
        }
        transform.position = spawnPoint;
        playerHealth.ResetHealth();
        gameManager.HandlePlayerDamage(player, playerHealth.Damage);
    }

    public void Die()
    {
        if (gameManager.isPreparing)
        {
            Debug.Log("Player is in preparation phase, cannot die.");
            Respawn(); // Respawn instead of dying
            return; // Ignore if in preparation phase
        }
        playerHealth.Lives--;
        gameManager.HandlePlayerDeath(player, playerHealth.Lives);        
        if (playerHealth.Lives > 0) { Respawn(); }
    }
}
