using UnityEngine;

public class PlayerSoundController : MonoBehaviour
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip stepSound; 
    [SerializeField] private AudioClip jumpSound; 
    [SerializeField] private AudioClip landSound; 
    [SerializeField] private AudioClip meleeSound; 
    [SerializeField] private AudioClip barrierSound; 
    [SerializeField] private AudioClip projectileSound; 
    [SerializeField] private AudioClip hitSound; 

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    public void PlayStep() => PlaySound(stepSound);
    public void PlayJump() => PlaySound(jumpSound);
    public void PlayLand() => PlaySound(landSound);
    public void PlayMelee() => PlaySound(meleeSound);
    public void PlayBarrier() => PlaySound(barrierSound);
    public void PlayProjectile() => PlaySound(projectileSound);
    public void PlayHit() => PlaySound(hitSound);

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }
}