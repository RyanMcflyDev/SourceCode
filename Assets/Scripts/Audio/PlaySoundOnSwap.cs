using UnityEngine;

// plays a sound every time byte swaps, kept apart from SwapEffect so the visuals and audio can change separately
[RequireComponent(typeof(AudioSource))]
public class PlaySoundOnSwap : MonoBehaviour
{
    [SerializeField] private AudioClip clip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.7f;
    // small random pitch change so repeated swaps don't sound identical
    [SerializeField, Range(0f, 0.3f)] private float pitchVariation = 0.05f;

    private AudioSource source;

    private void Awake() => source = GetComponent<AudioSource>();
    private void OnEnable() => GameEvents.PlayerSwapped += OnSwapped;
    private void OnDisable() => GameEvents.PlayerSwapped -= OnSwapped;

    private void OnSwapped(Transform target)
    {
        if (clip == null) return;
        source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        source.PlayOneShot(clip, volume);
    }
}
