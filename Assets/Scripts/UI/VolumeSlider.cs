using UnityEngine;
using UnityEngine.UI;

// master volume, saved between sessions. put it on a ui slider in the pause menu and the main menu
[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour
{
    private const string SaveKey = "MasterVolume";

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
    }

    // menus switch panels on and off, so refresh the handle every time it shows
    private void OnEnable()
    {
        slider.SetValueWithoutNotify(AudioListener.volume);
        slider.onValueChanged.AddListener(SetVolume);
    }

    private void OnDisable() => slider.onValueChanged.RemoveListener(SetVolume);

    private void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(SaveKey, value);
    }

    // applies the saved volume before the first scene, so it holds even if no menu is opened
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadSaved() => AudioListener.volume = PlayerPrefs.GetFloat(SaveKey, 1f);
}
