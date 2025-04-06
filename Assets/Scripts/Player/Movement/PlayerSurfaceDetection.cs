using UnityEngine;

public class PlayerSurfaceDetection : MonoBehaviour
{
    
    private bool onGround;
    private bool onWall;
    private bool previousGroundCheck;
    private float previousVelocity;
    private Vector2 facingDirection;
    private Vector3 groundOffset;
    private Vector3 wallOffset;
    private Rigidbody2D playerRb;
    private Animator playerAnim;
    [SerializeField] private BoxCollider2D playerCollider;
    [SerializeField] private Vector2 colliderSizeGround = new(1,1);
    [SerializeField] private Vector2 colliderSizeAir = new (0.6f, 0.6f);
    [SerializeField] private float colliderRadiusGround = 0;
    [SerializeField] private float colliderRadiusAir = 0.2f;


    [Header("Collider Settings")]
    [SerializeField][Tooltip("Length of the ground-checking collider")] private float groundLength = 0.95f;
    [SerializeField][Tooltip("Length of the ground-checking collider")] private float wallLength = 0.65f;
    [SerializeField][Tooltip("Distance between the ground-checking colliders")] private Vector3 colliderOffset;

    [Header("Layer Masks")]
    [SerializeField][Tooltip("Which layers are read as the ground")] private LayerMask groundLayer;

    private void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();
        playerAnim = GetComponent<Animator>();

        groundOffset = new Vector3(colliderOffset.x, 0, 0);
        wallOffset = new Vector3(0, colliderOffset.y, 0);
        previousGroundCheck = onGround;
        facingDirection = new Vector2(transform.localScale.x, 0);
    }

    private void Update()
    {
        facingDirection.x = transform.localScale.x;
    }

    private void FixedUpdate()
    {
        //Determine if the player is stood on objects on the ground layer, using a pair of raycasts
        onGround = Physics2D.Raycast(transform.position + groundOffset, Vector2.down, groundLength, groundLayer) || Physics2D.Raycast(transform.position - groundOffset, Vector2.down, groundLength, groundLayer);
        onWall = Physics2D.Raycast(transform.position + new Vector3(0f,-0.75f,0f) + wallOffset, facingDirection, wallLength, groundLayer) || Physics2D.Raycast(transform.position + new Vector3(0f, -0.75f, 0f) + - wallOffset, facingDirection, wallLength, groundLayer);

        playerAnim.SetBool("OnWall", onWall);

        if (onGround != previousGroundCheck)
        {
            previousGroundCheck = onGround;
            if (onGround)
            {
                previousVelocity = playerRb.linearVelocityX;
                playerCollider.size = colliderSizeGround;
                playerCollider.edgeRadius = colliderRadiusGround;
                playerAnim.SetBool("OnGround", true);
            }
            else
            {
                playerCollider.size = colliderSizeAir;
                playerCollider.edgeRadius = colliderRadiusAir;
                playerAnim.SetBool("OnGround", false);  
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            playerRb.linearVelocityX = previousVelocity;
        }
    }

    private void OnDrawGizmos()
    {
        // Draw the ground detection rays
        Gizmos.color = onGround ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position + groundOffset, transform.position + groundOffset + Vector3.down * groundLength);
        Gizmos.DrawLine(transform.position - groundOffset, transform.position - groundOffset + Vector3.down * groundLength);

        // Draw the wall detection rays
        Gizmos.color = onWall ? Color.blue : Color.red;
        Gizmos.DrawLine(transform.position + new Vector3(0f, -0.75f, 0f) + wallOffset, transform.position + new Vector3(0f, -0.75f, 0f) +  wallOffset + facingDirection.x * wallLength * Vector3.right);
        Gizmos.DrawLine(transform.position + new Vector3(0f, -0.75f, 0f) - wallOffset, transform.position + new Vector3(0f, -0.75f, 0f) - wallOffset + facingDirection.x * wallLength * Vector3.right);


    }

    //Send ground detection to other scripts
    public bool GetOnGround() { return onGround; }
    public bool GetOnWall() { return onWall; }
    public float GetFacingDirection() { return facingDirection.x; }
}
