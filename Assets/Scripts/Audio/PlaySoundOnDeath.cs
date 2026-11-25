using UnityEngine;

// plays a sound when byte dies; respawn follows 0.25 s later, so one clip covers both
[RequireComponent(typeof(AudioSource))]
public class PlaySoundOnDeath : MonoBehaviour
{
    [SerializeField] private AudioClip clip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.8f;

    private AudioSource source;

    private void Awake() => source = GetComponent<AudioSource>();
    private void OnEnable() => GameEvents.PlayerDied += OnPlayerDied;
    private void OnDisable() => GameEvents.PlayerDied -= OnPlayerDied;

    private void OnPlayerDied(string cause)
    {
        if (clip != null) source.PlayOneShot(clip, volume);
    }
}
