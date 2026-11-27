using UnityEngine;
using UnityEngine.EventSystems;

// plays a sound when the mouse moves onto this button, through the menu's shared audio source
public class PlaySoundOnHover : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip clip;
    [SerializeField] private float volume = 1f;

    // one shot, so it doesn't swap out the click clip the source already holds
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (source != null && clip != null) source.PlayOneShot(clip, volume);
    }
}
