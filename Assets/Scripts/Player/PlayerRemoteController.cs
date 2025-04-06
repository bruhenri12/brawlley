using Brawlley;
using UnityEngine;

public class PlayerRemoteController : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private PlayerJump playerJump;
    private PlayerDash playerDash;
    private PlayerParry playerParry;
    private PlayerSpell playerSpell;
    private PlayerMelee playerMelee;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerJump = GetComponent<PlayerJump>();
        playerDash = GetComponent<PlayerDash>();
        playerParry = GetComponent<PlayerParry>();
        playerSpell = GetComponent<PlayerSpell>();
        playerMelee = GetComponent<PlayerMelee>();
    }
    


    public void DoAJump()
    {
        playerJump.OnRemoteJump();
    }

    public void DoADash()
    {
        playerDash.OnRemoteDash();
    }

    public void OnRemoteParry()
    {
        playerParry.OnRemoteParry();
    }

}
