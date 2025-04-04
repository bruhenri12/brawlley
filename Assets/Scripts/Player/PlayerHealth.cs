using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] int playerLives = 3;
    [SerializeField] float damage;

    private void Start()
    {
        ResetHealth();
    }

    public void ResetHealth()
    {
        damage = 0;
    }

    public float Damage { get => damage; set => damage = value; }

    public int Lives { get => playerLives; set => playerLives = value; }
}
