using UnityEngine;

// every few seconds the object snaps out of place for a moment then settles, a cheap glitch for glitch and corrupted enemies
public class GlitchJitter : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float minWait = 1.5f;
    [SerializeField] private float maxWait = 4f;
    [SerializeField] private float glitchTime = 0.12f;

    [Header("Strength")]
    [SerializeField] private float positionAmount = 0.15f;
    [SerializeField] private float scaleAmount = 0.3f;

    private Vector3 startPosition;
    private Vector3 startScale;
    private float nextGlitch;
    private float glitchEnd;
    private bool wasGlitching;

    private void OnEnable()
    {
        startPosition = transform.localPosition;
        startScale = transform.localScale;
        nextGlitch = Time.time + Random.Range(minWait, maxWait);
    }

    private void Update()
    {
        if (Time.time >= nextGlitch)
        {
            glitchEnd = Time.time + glitchTime;
            nextGlitch = glitchEnd + Random.Range(minWait, maxWait);
        }

        bool glitching = Time.time < glitchEnd;

        if (glitching)
        {
            // a new random offset every frame while it lasts
            transform.localPosition = startPosition + Random.insideUnitSphere * positionAmount;
            transform.localScale = startScale * (1f + Random.Range(-scaleAmount, scaleAmount));
        }
        else if (wasGlitching)
        {
            transform.localPosition = startPosition;
            transform.localScale = startScale;
        }

        wasGlitching = glitching;
    }
}
