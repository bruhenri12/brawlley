using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerJuice : MonoBehaviour
{
    [Header("Components - General")]
    [SerializeField] private Transform visualTransform;

    [Header("Components - Particles")]
    [SerializeField] private ParticleSystem moveParticles;
    [SerializeField] private ParticleSystem jumpParticles;
    [SerializeField] private ParticleSystem wallJumpParticles;
    [SerializeField] private ParticleSystem landParticles;
    [SerializeField] private ParticleSystem dashParticles;
    [SerializeField] private ParticleSystem dashReloadParticles;
    [SerializeField] private ParticleSystem immuneParticles;
    [SerializeField] private ParticleSystem gravityCancelParticles;
    [SerializeField] private ParticleSystem spellHitParticles;
    [SerializeField] private ParticleSystem meleeHitParticles;
    [SerializeField] private ParticleSystem riposteParticles;

    [Header("Components - Audio")]
    [SerializeField] AudioSource jumpSFX;
    [SerializeField] AudioSource landSFX;

    [Header("Tilting")]
    [SerializeField, Tooltip("How far should the character tilt?")] public float maxTilt;
    [SerializeField, Tooltip("How fast should the character tilt?")] public float tiltSpeed;

    private Rigidbody2D playerRb;
    private PlayerSurfaceDetection surfaceDetection;
    private Squash squash;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        surfaceDetection = GetComponent<PlayerSurfaceDetection>();
        playerRb = GetComponent<Rigidbody2D>();
        squash = GetComponent<Squash>();
    }

    // Update is called once per frame
    void Update()
    {
        TiltCharacter();

        MovementJuice();
    }
    private void TiltCharacter()
    {
        //See which direction the character is currently running towards, and tilt in that direction
        float directionToTilt = 0;
        if (playerRb.linearVelocityX != 0)
        {
            directionToTilt = Mathf.Sign(playerRb.linearVelocityX);
        }

        //Create a vector that the character will tilt towards
        Vector3 targetRotVector = new(0, 0, Mathf.Lerp(-maxTilt, maxTilt, Mathf.InverseLerp(-1, 1, directionToTilt)));

        //And then rotate the character in that direction
        if (directionToTilt == 0)
        {
            visualTransform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            visualTransform.rotation = Quaternion.RotateTowards(visualTransform.rotation,
                                                            Quaternion.Euler(-targetRotVector),
                                                            tiltSpeed * Time.deltaTime);
        }
            
        
    }

    public void MovementJuice()
    {
        if (Mathf.Abs(playerRb.linearVelocityX) > 0)
        {
            if (surfaceDetection.GetOnGround()) { moveParticles.Play(); }
        }
        else
        {
            moveParticles.Stop();
        }
    }

    public void LandingJuice()
    {
        squash.PlayLandSquash();
        landParticles.Play();
    }

    public void JumpJuice()
    {
        if (surfaceDetection.GetOnWall() && !surfaceDetection.GetOnGround())
        {
            wallJumpParticles.Play();
        }
        else
        {
            squash.PlayJumpStretch();
            jumpParticles.Play();
            //jumpSFX.Play();
        }

    }

    public void DashJuice(Vector2 direction)
    {
        if (direction == Vector2.zero)
        {
            immuneParticles.Play();
        }
        else
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            ParticleSystem particles = Instantiate(dashParticles, transform.position + dashParticles.transform.position, Quaternion.Euler(0,0,angle));
        }
        
    }

    public void ReloadDashJuice()
    {
        dashReloadParticles.Play();
    }

    public void ParryJuice()
    {
        if (!surfaceDetection.GetOnGround())
        {
            gravityCancelParticles.Play();
        }
    }

    public void SpellHurtJuice(Vector3 position)
    {
        ParticleSystem ps = Instantiate(spellHitParticles, position, transform.rotation);
        ps.Play();
        Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
    }

    public void MeleeHurtJuice(Vector3 position)
    {
        ParticleSystem ps = Instantiate(meleeHitParticles, position, transform.rotation);
        ps.Play();
        Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
    }

    public void RiposteJuice(Vector3 position)
    {
        ParticleSystem ps = Instantiate(riposteParticles, position, transform.rotation);
        ps.Play();
        Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
    }

    public void SpellReloadJuice()
    {

    }

}
