using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.5f;

    private Rigidbody2D playerRb;
    private Animator playerAnim;
    private bool isDashing;
    private bool canDash = true;

    private Vector2 dashDirection;
    private Vector2 inputDirection;
    private float dashEndTime;
    private float dashCooldownEndTime;

    private PlayerSurfaceDetection surfaceDetector;

    public Vector2 Direction { set => inputDirection = value; }
    public bool IsDashing { get => isDashing; }
    public bool GravityCancel { get => isDashing && dashDirection == Vector2.zero; }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (!canDash || isDashing) return;

        if (context.started)
        {
            StartDash(inputDirection);
            playerAnim.SetTrigger("DashTrigger");
            playerAnim.ResetTrigger("EndDashTrigger");
        }
    }

    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        playerAnim = GetComponent<Animator>();
        surfaceDetector = GetComponent<PlayerSurfaceDetection>();
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            if (Time.time >= dashEndTime)
            {
                EndDash();
            }
            else
            {
                playerRb.linearVelocity = dashDirection * dashSpeed;
            }
        }

        // Atualiza se pode ou não dar dash novamente
        if (!canDash && Time.time >= dashCooldownEndTime)
        {
            canDash = true;
        }
    }

    private void StartDash(Vector2 direction)
    {
        isDashing = true;
        canDash = false;
        dashDirection = direction;
        dashEndTime = Time.time + dashDuration;
        dashCooldownEndTime = Time.time + dashCooldown;

        playerRb.gravityScale = 0;
    }

    public void ExtendDash(float extraTime)
    {
        if (isDashing)
        {
            dashEndTime += extraTime;
        }
    }

    private void EndDash()
    {
        isDashing = false;
        playerRb.gravityScale = 1;
        playerAnim.SetTrigger("EndDashTrigger");
    }
}