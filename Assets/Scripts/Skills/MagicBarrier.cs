using Brawlley.Attacks;
using UnityEngine;

public class MagicBarrier : MonoBehaviour
{
    [SerializeField] float lifetime = 10f;
    [SerializeField] BoxCollider2D parryCollider;
    [SerializeField] CapsuleCollider2D wallCollider;
    public float instantiationTime;

    void Start()
    {
        // Destroy this game object after 'lifetime' seconds
        Destroy(gameObject, lifetime);
        instantiationTime = Time.time;
    }

    // Usando OnTriggerEnter2D para lidar com as colis�es com Proj�teis
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Spell spell = collision.gameObject.GetComponent<Spell>();

        if (parryCollider.IsTouching(collision))
        {
            spell.Parry();
            return;
        }
        else if (wallCollider.IsTouching(collision))
        {
            //Precisa Trabalhar Isso Depois
            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
        
        
    }

    // Usando OnCollisionEnter2D para lidar com as colis�es entre barreiras
    private void OnCollisionEnter2D(Collision2D collision)
    {
        MagicBarrier otherObject = collision.gameObject.GetComponent<MagicBarrier>();

        if (otherObject != null)
        {
            if (otherObject.instantiationTime < this.instantiationTime)
            {
                Destroy(otherObject.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }
    }

}
