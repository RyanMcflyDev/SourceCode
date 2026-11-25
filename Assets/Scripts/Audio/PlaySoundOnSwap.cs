using System.Collections;
using UnityEngine;

// plays a sound every time byte swaps, kept apart from SwapEffect so the visuals and audio can change separately
// the sliders trim and speed up a long clip without editing the file; tune them in play mode, then Copy Component
[RequireComponent(typeof(AudioSource))]
public class PlaySoundOnSwap : MonoBehaviour
{
    [SerializeField] private AudioClip clip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.7f;

    [Header("Tuning")]
    // 1 is normal; higher is faster and higher pitched, Unity can't change one without the other
    [SerializeField, Range(0.5f, 2f)] private float pitch = 1f;
    // skip this far into the clip, in seconds of the original clip
    [SerializeField, Min(0f)] private float startAt;
    // stop after this many real seconds, 0 plays to the end
    [SerializeField, Min(0f)] private float maxLength;
    // fade over the last part of max length so the cut doesn't click
    [SerializeField, Min(0f)] private float fadeOut = 0.1f;
    // small random pitch change so repeated swaps don't sound identical
    [SerializeField, Range(0f, 0.3f)] private float pitchVariation = 0.05f;

    private AudioSource source;
    private Coroutine playing;

    private void Awake() => source = GetComponent<AudioSource>();
    private void OnEnable() => GameEvents.PlayerSwapped += OnSwapped;

    private void OnDisable()
    {
        GameEvents.PlayerSwapped -= OnSwapped;
        source.Stop();
    }

    private void OnSwapped(Transform target)
    {
        if (clip == null) return;

        // a new swap cuts off the last one rather than stacking
        if (playing != null) StopCoroutine(playing);

        source.clip = clip;
        source.pitch = pitch + Random.Range(-pitchVariation, pitchVariation);
        source.volume = volume;
        source.time = Mathf.Clamp(startAt, 0f, clip.length - 0.01f);
        source.Play();

        playing = maxLength > 0f ? StartCoroutine(StopAfter(maxLength)) : null;
    }

    private IEnumerator StopAfter(float length)
    {
        float fade = Mathf.Min(fadeOut, length);
        yield return new WaitForSeconds(length - fade);

        for (float t = 0f; t < fade; t += Time.deltaTime)
        {
            source.volume = volume * (1f - t / fade);
            yield return null;
        }

        source.Stop();
        source.volume = volume;
        playing = null;
    }
}
