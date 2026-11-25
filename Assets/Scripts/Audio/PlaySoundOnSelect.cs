using UnityEngine;

// plays a sound when byte selects something to swap with, and a different one when he can't
[RequireComponent(typeof(AudioSource))]
public class PlaySoundOnSelect : MonoBehaviour
{
    [SerializeField] private AudioClip selectClip;
    [SerializeField] private AudioClip blockedClip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.7f;

    private AudioSource source;

    private void Awake() => source = GetComponent<AudioSource>();

    private void OnEnable()
    {
        GameEvents.SelectionChanged += OnSelectionChanged;
        GameEvents.SelectionBlocked += OnSelectionBlocked;
    }

    private void OnDisable()
    {
        GameEvents.SelectionChanged -= OnSelectionChanged;
        GameEvents.SelectionBlocked -= OnSelectionBlocked;
    }

    // null means the selection was dropped (swap done, cancel, death), which stays silent
    private void OnSelectionChanged(Transform target)
    {
        if (target != null) Play(selectClip);
    }

    private void OnSelectionBlocked() => Play(blockedClip);

    private void Play(AudioClip clip)
    {
        if (clip != null) source.PlayOneShot(clip, volume);
    }
}
