using KinoGlitch;
using UnityEngine;

// short glitch burst when byte dies, it covers the room snapping back
[RequireComponent(typeof(AnalogGlitchController))]
public class DeathGlitch : MonoBehaviour
{
    [SerializeField] private float duration = 0.5f;

    [Header("Peak Strength (0 to 1)")]
    [SerializeField, Range(0f, 1f)] private float colorDrift = 0.6f;
    [SerializeField, Range(0f, 1f)] private float scanLineJitter = 0.5f;
    [SerializeField, Range(0f, 1f)] private float horizontalRipple = 0.4f;

    private AnalogGlitchController glitch;
    private float endTime;
    private bool wasPlaying;

    private void Awake() => glitch = GetComponent<AnalogGlitchController>();

    private void OnEnable() => GameEvents.PlayerDied += OnPlayerDied;
    private void OnDisable() => GameEvents.PlayerDied -= OnPlayerDied;

    private void OnPlayerDied(string cause) => endTime = Time.unscaledTime + duration;

    private void Update()
    {
        // unscaled so bullet time doesn't stretch it
        float strength = Mathf.Clamp01((endTime - Time.unscaledTime) / duration);
        bool playing = strength > 0f;
        if (!playing && !wasPlaying) return;

        // squared so it hits hard then tails off quickly
        strength *= strength;
        glitch.ColorDrift = colorDrift * strength;
        glitch.ScanLineJitter = scanLineJitter * strength;
        glitch.HorizontalRipple = horizontalRipple * strength;

        wasPlaying = playing;
    }
}
