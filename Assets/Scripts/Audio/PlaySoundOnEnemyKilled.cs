using System.Collections;
using UnityEngine;

// plays a sound when an enemy starts its death dissolve; lives on byte so it isn't cut off when the enemy switches off
// same tuning sliders as PlaySoundOnSwap, tune in play mode then Copy Component
[RequireComponent(typeof(AudioSource))]
public class PlaySoundOnEnemyKilled : MonoBehaviour
{
    [SerializeField] private AudioClip clip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.25f;

    [Header("Tuning")]
    // 1 is normal; higher is faster and higher pitched
    [SerializeField, Range(0.5f, 2f)] private float pitch = 1.15f;
    // random change each time so repeated kills don't sound identical
    [SerializeField, Range(0f, 0.3f)] private float pitchVariation = 0.08f;
    // skip this far into the clip, in seconds of the original clip
    [SerializeField, Min(0f)] private float startAt;
    // stop after this many real seconds, 0 plays to the end
    [SerializeField, Min(0f)] private float maxLength = 0.9f;
    // fade over the last part of max length so the cut doesn't click
    [SerializeField, Min(0f)] private float fadeOut = 0.3f;

    private AudioSource source;
    private Coroutine playing;

    private void Awake() => source = GetComponent<AudioSource>();
    private void OnEnable() => GameEvents.EnemyKilled += OnEnemyKilled;

    private void OnDisable()
    {
        GameEvents.EnemyKilled -= OnEnemyKilled;
        source.Stop();
    }

    private void OnEnemyKilled(Transform enemy)
    {
        if (clip == null) return;
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
