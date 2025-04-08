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
    private PlayerJuice juice;
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
        if (!canDash || isDashing || surfaceDetector.GetOnWall()) return;

        if (context.started)
        {
            StartDash(inputDirection);
            juice.DashJuice(dashDirection);
            playerAnim.SetBool("IsDashing", true);
        }
    }

    public void OnRemoteDash()
    {
        if (!canDash || isDashing || surfaceDetector.GetOnWall()) return;
        StartDash(inputDirection);
        juice.DashJuice(dashDirection);
        playerAnim.SetBool("IsDashing", true);
    }

    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        playerAnim = GetComponentInChildren<Animator>();
        surfaceDetector = GetComponent<PlayerSurfaceDetection>();
        juice = GetComponent<PlayerJuice>();
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

        // Atualiza se pode ou n�o dar dash novamente
        if (!canDash && Time.time >= dashCooldownEndTime)
        {
            canDash = true;
            juice.ReloadDashJuice();
        }
    }

    private void StartDash(Vector2 direction)
    {
        float neutralDashModifier = 0;
        if (direction == Vector2.zero) { neutralDashModifier = 0.4f; }

        
        isDashing = true;
        canDash = false;
        dashDirection = direction;
        dashEndTime = Time.time + dashDuration + neutralDashModifier;
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
        playerAnim.SetBool("IsDashing", false);
    }
}