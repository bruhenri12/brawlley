using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]

    private Rigidbody2D playerRb;
    private Animator playerAnim;
    PlayerSurfaceDetection surfaceDetector;

    [Header("Movement Stats")]
    [SerializeField, Range(0f, 20f)][Tooltip("Maximum movement speed")] public float maxSpeed = 10f;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to reach max speed")] public float maxAcceleration = 52f;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop after letting go")] public float maxDecceleration = 52f;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop when changing direction")] public float maxTurnSpeed = 80f;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to reach max speed when in mid-air")] public float maxAirAcceleration;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop in mid-air when no direction is used")] public float maxAirDeceleration;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop when changing direction when in mid-air")] public float maxAirTurnSpeed = 80f;
    [SerializeField][Tooltip("Friction to apply against movement on stick")] private float friction;

    [Header("Options")]
    [Tooltip("When false, the charcter will skip acceleration and deceleration and instantly move and stop")] public bool useAcceleration;

    [Header("Calculations")]
    public float directionX;
    private Vector2 desiredVelocity;
    public Vector2 velocity;
    private float maxSpeedChange;
    private float acceleration;
    private float deceleration;
    private float turnSpeed;

    [Header("Current State")]
    private bool onGround;
    private bool onWall;
    private bool pressingKey;
    private bool canMove = true;

    public bool CanMove { get => canMove; set => canMove = value; }
    public float Direction { set => directionX = value; }

    private void Awake()
    {
        //Find the character's Rigidbody and ground detection script
        playerRb = GetComponent<Rigidbody2D>();
        playerAnim = GetComponent<Animator>();
        surfaceDetector = GetComponent<PlayerSurfaceDetection>();
    }

    private void Update()
    {
        if (directionX != 0)
        {
            transform.localScale = new Vector3(directionX > 0 ? 1 : -1, 1, 1);
            pressingKey = true;
        }
        else
        {
            pressingKey = false;
        }

        //Calculate's the character's desired velocity - which is the direction you are facing, multiplied by the character's maximum speed
        //Friction is not used in this game
        desiredVelocity = new Vector2(directionX, 0f) * Mathf.Max(maxSpeed - friction, 0f);

    }

    private void FixedUpdate()
    {
        onGround = surfaceDetector.GetOnGround();
        onWall = surfaceDetector.GetOnWall();
        velocity = playerRb.linearVelocity;

        playerAnim.SetFloat("Speed", Mathf.Abs(velocity.x));
        playerAnim.SetFloat("VerticalSpeed", velocity.y);

        RunWithAcceleration();
    }

    private void RunWithAcceleration()
    {
        //Set our acceleration, deceleration, and turn speed stats, based on whether we're on the ground on in the air

        acceleration = onGround ? maxAcceleration : maxAirAcceleration;
        deceleration = onGround ? maxDecceleration : maxAirDeceleration;
        turnSpeed = onGround ? maxTurnSpeed : maxAirTurnSpeed;

        if (pressingKey && canMove)
        {
            //If the sign (i.e. positive or negative) of our input direction doesn't match our movement, it means we're turning around and so should use the turn speed stat.
            if (Mathf.Sign(directionX) != Mathf.Sign(velocity.x))
            {
                maxSpeedChange = turnSpeed * Time.deltaTime;
            }
            else
            {
                //If they match, it means we're simply running along and so should use the acceleration stat
                maxSpeedChange = acceleration * Time.deltaTime;
            }
        }
        else
        {
            //And if we're not pressing a direction at all, use the deceleration stat
            maxSpeedChange = deceleration * Time.deltaTime;
        }

        //Move our velocity towards the desired velocity, at the rate of the number calculated above
        

        if (!onWall)
        {
            if (!canMove)
            {
                velocity.x = Mathf.MoveTowards(velocity.x, 0, maxSpeedChange);
            }
            else
            {
                velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
            }
        }

        //Update the Rigidbody with this new velocity
        playerRb.linearVelocityX = velocity.x;

    }

    private void RunWithoutAcceleration()
    {
        //If we're not using acceleration and deceleration, just send our desired velocity (direction * max speed) to the Rigidbody
        if (!onWall)
        {
            velocity.x = desiredVelocity.x;
        }

        playerRb.linearVelocity = velocity;
    }

}
