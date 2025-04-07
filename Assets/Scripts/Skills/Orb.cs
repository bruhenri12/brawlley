using System.Xml.Linq;
using UnityEngine;

public class Orb : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public ParticleSystem particlePrefab;


    public void Disable(float cooldown)
    {
        if (particlePrefab != null)
        {
            ParticleSystem ps = Instantiate(particlePrefab, transform.position, transform.rotation);
            ps.Play();
            Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
        }

        gameObject.SetActive(false);

        Invoke(nameof(Enable), cooldown - 0.2f);
    }

    private void Enable()
    {
        gameObject.SetActive(true);
    }
}
