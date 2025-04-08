using System;
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

    public void GetHit(Vector2 direction, float damage = 10, float knockbackForce = 1f)
    {
        playerHealth.Damage += damage; // Increase damage
        Debug.Log("Player Damage taken: " + playerHealth.Damage);
        gameManager.HandlePlayerDamage(player, playerHealth.Damage);

        playerRb.linearVelocity = playerHealth.Damage * direction;
    }

    // OnTriggerEnter2D � usado para lidar a colis�o entre o player e o proj�til (trigger)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("WorldBounds")) { return; }

        if (collision.CompareTag("Spell"))
        {
            Spell spell = collision.GetComponent<Spell>();
            if (spell.ignoreTeam == player.Team.name) { return; }
            Debug.Log("Apanhou Spell");
            GetHit(spell.direction, spell.damage, spell.knockbackForce);
            Destroy(collision.gameObject);
        }
        else if (collision.CompareTag("Melee"))
        {
            Melee meleeAttack = collision.GetComponent<Melee>();
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
        playerHealth.ResetHealth();
        playerRb.linearVelocity = Vector2.zero;
        Vector3 spawnPoint = gameManager.GetRandomSpawnPoint().position;
        if (gameManager.map == Map.Volley)
        {
            spawnPoint = player.spawnPoint.position;
        }
        transform.position = spawnPoint;
        gameManager.HandlePlayerDamage(player, playerHealth.Damage);
    }

    public void Die()
    {
        playerHealth.Lives--;
        gameManager.HandlePlayerDeath(player, playerHealth.Lives);        
        if (playerHealth.Lives > 0) { Respawn(); }
    }
}
