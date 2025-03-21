using Brawlley;
using Brawlley.Attacks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class PlayerController : MonoBehaviour
{
    #region Components
    [Header("Components")]
    //GameInputs gameInputs;
    private PlayerInput playerInput;
    private PlayerMovement playerMovement;
    private PlayerJump playerJump;
    private PlayerDash playerDash;
    private PlayerParry playerParry;
    private PlayerSpell playerSpell;
    private PlayerMelee playerMelee;
    #endregion

    #region Data
    Vector2 playerDirection;
    #endregion

    #region Properties
    //public GameInputs GameInputs => gameInputs;
    public Vector2 PlayerDirection => playerDirection;
    #endregion

    #region MonoBehaviour Lifecycle Methods
    void Awake()
    {
        //gameInputs = new GameInputs(); // Initialize input system
        playerInput = GetComponent<PlayerInput>(); // Get reference to player input
        // Get references to your movement and jump scripts
        playerJump = GetComponent<PlayerJump>();
        playerDash = GetComponent<PlayerDash>();
        playerMovement = GetComponent<PlayerMovement>();
        playerParry = GetComponent<PlayerParry>();
        playerSpell = GetComponent<PlayerSpell>();
        playerMelee = GetComponent<PlayerMelee>();
    }

    void OnEnable()
    {
        // Enable player input actions dynamically
        playerInput.actions.Enable();

        playerInput.actions["Movement"].performed += SetDirection;
        playerInput.actions["Movement"].canceled += SetDirection;

        playerInput.actions["Jump"].started += playerJump.OnJump;
        playerInput.actions["Dash"].started += playerDash.OnDash;

        playerInput.actions["Spell"].started += OnAiming;
        playerInput.actions["Spell"].canceled += OnStopAiming;
        playerInput.actions["Spell"].canceled += playerSpell.OnAttack;

        playerInput.actions["Parry"].started += playerParry.OnParry;
        playerInput.actions["Melee"].started += playerMelee.OnAttack;

        // Debugging control schemes
        Debug.Log($"{gameObject.name} using {playerInput.currentControlScheme}");
    }

    void OnDisable()
    {
        playerInput.actions["Movement"].performed -= SetDirection;
        playerInput.actions["Movement"].canceled -= SetDirection;

        playerInput.actions["Jump"].started -= playerJump.OnJump;
        playerInput.actions["Dash"].started -= playerDash.OnDash;

        playerInput.actions["Spell"].started -= OnAiming;
        playerInput.actions["Spell"].canceled -= OnStopAiming;
        playerInput.actions["Spell"].canceled -= playerSpell.OnAttack;

        playerInput.actions["Parry"].started -= playerParry.OnParry;
        playerInput.actions["Melee"].started -= playerMelee.OnAttack;

        playerInput.actions.Disable();
    }
    #endregion

    #region Player Controller Methods
    void SetDirection(InputAction.CallbackContext context)
    {
        playerDirection = context.ReadValue<Vector2>();
        if (playerMovement != null) playerMovement.Direction = playerDirection.x;
        if (playerJump != null) playerJump.Direction = Mathf.Min(0, playerDirection.y);
        if (playerDash != null) playerDash.Direction = playerDirection.normalized;
        if (playerMelee != null) playerMelee.Direction = playerDirection;
        if (playerMelee != null) playerMelee.UpdateDirection(playerDirection.x);
        if (playerSpell != null) playerSpell.Direction = playerDirection;
    }

    void OnAiming(InputAction.CallbackContext context)
    {
        playerMovement.CanMove = false;
        playerInput.actions["Jump"].started -= playerJump.OnJump;
        playerInput.actions["Dash"].started -= playerDash.OnDash;
        playerInput.actions["Parry"].started -= playerParry.OnParry;
        playerInput.actions["Melee"].started -= playerMelee.OnAttack;
    }

    void OnStopAiming(InputAction.CallbackContext context)
    {
        playerMovement.CanMove = true;
        playerInput.actions["Jump"].started += playerJump.OnJump;
        playerInput.actions["Dash"].started += playerDash.OnDash;
        playerInput.actions["Parry"].started += playerParry.OnParry;
        playerInput.actions["Melee"].started += playerMelee.OnAttack;
    }

    public Vector2 GetDirection()
    {
        return playerDirection;
    }
    #endregion
}
