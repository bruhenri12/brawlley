using Brawlley.Attacks;
using UnityEngine;

public class SpellCollision2D : MonoBehaviour
{
    public GameManager gameManager;
    public string teamName;
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Spell"))
        {
            if (collision.TryGetComponent(out Spell spell))
            {
                if (spell.ignoreTeam == teamName)
                {
                    gameManager.TeamFault(spell.ignoreTeam);
                }
                else
                {
                    gameManager.ScorePoint(spell.ignoreTeam);
                }
            }
            Destroy(collision.gameObject);
        }
    }
}
