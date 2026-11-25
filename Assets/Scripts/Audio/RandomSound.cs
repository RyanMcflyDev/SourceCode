using UnityEngine;

// plays one clip picked at random from a list, never the same one twice in a row
// call Play from an event, e.g. a pressure plate's On Pressed
[RequireComponent(typeof(AudioSource))]
public class RandomSound : MonoBehaviour
{
    [SerializeField] private AudioClip[] clips;
    [SerializeField, Range(0f, 1f)] private float volume = 0.6f;
    // small random pitch change so repeats don't sound identical
    [SerializeField, Range(0f, 0.3f)] private float pitchVariation = 0.05f;

    private AudioSource source;
    private int lastIndex = -1;

    private void Awake() => source = GetComponent<AudioSource>();

    public void Play()
    {
        if (clips == null || clips.Length == 0) return;

        int index = Random.Range(0, clips.Length);
        if (clips.Length > 1 && index == lastIndex) index = (index + 1) % clips.Length;
        lastIndex = index;

        source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        source.PlayOneShot(clips[index], volume);
    }
}
