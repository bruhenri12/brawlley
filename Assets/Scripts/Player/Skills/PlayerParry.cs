using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerParry : MonoBehaviour
{

    [SerializeField] GameObject barrierPrefab;
    [SerializeField] float horizontalOffset = 1f;
    [SerializeField] float verticalOffset = 1f;
    [SerializeField] float parryCooldown = 1f;
    private PlayerSurfaceDetection surfaceDetector;
    private PlayerDash dash;
    private PlayerController playerController;
    private Animator playerAnim;
    private PlayerJuice juice;
    private bool canParry;
    //private bool wasDashing;

    private void Start()
    {
        surfaceDetector = GetComponent<PlayerSurfaceDetection>();
        dash = GetComponent<PlayerDash>();
        playerAnim = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        juice = GetComponent<PlayerJuice>();
        canParry = true;
    }

    public void OnParry(InputAction.CallbackContext context)
    {
        // O player só pode subir uma barreira no chão ou se der um Dash neutro (Gravity Cancel)
        if (context.started && canParry && (surfaceDetector.GetOnGround() || dash.GravityCancel))
        {
            playerController.DisableMovement();
            playerAnim.SetTrigger("ParryTrigger");
            
            StartCoroutine(Cooldown());

            //Ficar um tempinho a mais no ar se usar o gravity cancel
            if (dash.IsDashing)
            {
                //dash.ExtendDash(0.3f);
                playerAnim.SetBool("IsDashing", false);
                juice.ParryJuice();
            }
        }
    }

    public void OnRemoteParry()
    {
        if (canParry && (surfaceDetector.GetOnGround() || dash.GravityCancel))
        {
            playerController.DisableMovement();
            playerAnim.SetTrigger("ParryTrigger");

            StartCoroutine(Cooldown());

            //Ficar um tempinho a mais no ar se usar o gravity cancel
            if (dash.IsDashing)
            {
                dash.ExtendDash(0.3f);
            }
        }
    }


    // Gerar uma barreira na frente do player numa margem controlada pelo barrier Offset
    private Vector3 GetBarrierPosition()
    {
        return new Vector3(transform.position.x + horizontalOffset * surfaceDetector.GetFacingDirection(), transform.position.y - verticalOffset, transform.position.z);
    }

    public void SummonBarrier()
    {
        Instantiate(barrierPrefab, GetBarrierPosition(), Quaternion.identity);
        playerController.EnableMovement();
        

    }
    private IEnumerator Cooldown()
    {
        canParry = false;
        yield return new WaitForSeconds(parryCooldown);
        canParry = true;
    }

}
