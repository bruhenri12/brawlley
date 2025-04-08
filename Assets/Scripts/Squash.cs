using UnityEngine;

public class Squash: MonoBehaviour
{
    [SerializeField] private Transform visualTransform;

    [Header("Stretch on Jump")]
    [SerializeField] private Vector3 stretchScale = new Vector3(0.8f, 1.2f, 1f);
    [SerializeField] private float stretchDuration = 0.1f;

    [Header("Squash on Land")]
    [SerializeField] private Vector3 squashScale = new Vector3(1.2f, 0.8f, 1f);
    [SerializeField] private float squashDuration = 0.1f;

    private Coroutine squashRoutine;

    public void PlayJumpStretch()
    {
        PlaySquash(stretchScale, stretchDuration);
    }

    public void PlayLandSquash()
    {
        PlaySquash(squashScale, squashDuration);
    }

    private void PlaySquash(Vector3 targetScale, float duration)
    {
        if (squashRoutine != null)
            StopCoroutine(squashRoutine);
        squashRoutine = StartCoroutine(SquashCoroutine(targetScale, duration));
    }

    private System.Collections.IEnumerator SquashCoroutine(Vector3 targetScale, float duration)
    {
        visualTransform.localScale = targetScale;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            visualTransform.localScale = Vector3.Lerp(targetScale, Vector3.one, elapsed / duration);
            yield return null;
        }

        visualTransform.localScale = Vector3.one;
    }
}