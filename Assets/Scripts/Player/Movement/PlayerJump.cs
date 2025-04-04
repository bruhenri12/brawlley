using NUnit.Framework.Constraints;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJump : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D playerRb;
    private Animator playerAnim;
    private PlayerSurfaceDetection surfaceDetector;
    private PlayerDash dash;

    [Header("Jump Settings")]
    [SerializeField, Range(1f, 10f)] private float jumpHeight = 5f;
    [SerializeField, Range(0.1f, 1f)] private float timeToJumpApex = 0.4f;
    [SerializeField, Range(0.1f, 30f)] private float maxFallingSpeed = 20;
    [SerializeField, Range(1f, 5f)] private float fallingSpeedModifier = 2;
    [SerializeField, Range(0.1f, 30f)] private float wallFallingSpeed= 10;
    [SerializeField, Range(1f, 30f)] private float wallJumpStrength = 15;
    [SerializeField, Range(0, 5)] private int maxAirJumps = 1;

    private float jumpSpeed;
    private int JumpsRemaining;
    private float gravityScale;
    private float verticalDirection;

    public float Direction { set => verticalDirection = value; }
    public void OnJump(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                if (JumpsRemaining > 0)
                {
                    DoAJump();
                }
            }
        }

    #region MonoBehaviour Lifecycle Methods
    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        playerAnim = GetComponent<Animator>();
        surfaceDetector = GetComponent<PlayerSurfaceDetection>();
        dash = GetComponent<PlayerDash>();
        HandleJumpPhysics();
     
    }

    private void FixedUpdate()
    {
        if (dash.IsDashing)
        {
            playerRb.gravityScale = 0;
            return;
        }

        HandleJumpPhysics();

        if (surfaceDetector.GetOnGround() || surfaceDetector.GetOnWall())
        {
            JumpsRemaining = maxAirJumps;
        }

        HandleFallingSpeed();
    }
    #endregion

    private void HandleJumpPhysics()
    {
        float gravity = -(2 * jumpHeight) / (timeToJumpApex * timeToJumpApex);
        gravityScale = gravity / Physics2D.gravity.y;
        playerRb.gravityScale = gravityScale;

        jumpSpeed = Mathf.Abs(gravity) * timeToJumpApex;
    }

    private void DoAJump()
    {
        if(surfaceDetector.GetOnWall() && !surfaceDetector.GetOnGround())
        {
            playerRb.linearVelocity = new Vector2(-wallJumpStrength * surfaceDetector.GetFacingDirection(), jumpSpeed);
        }

        playerRb.linearVelocityY = jumpSpeed;
        playerAnim.SetTrigger("JumpTrigger");

        JumpsRemaining--;
    }

    private void HandleFallingSpeed()
    {
        // If on wall, fall slower
        if (surfaceDetector.GetOnWall())
        {
            playerRb.linearVelocityY = Mathf.Max(playerRb.linearVelocity.y, -wallFallingSpeed);
        }
        // Since players can be in the air for a while, we want to make sure they don't fall too fast
        else
        {
            playerRb.linearVelocityY = Mathf.Max(playerRb.linearVelocity.y, -maxFallingSpeed) 
                                     + verticalDirection * fallingSpeedModifier;
        }
    }
}