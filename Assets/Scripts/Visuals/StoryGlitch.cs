using KinoGlitch;
using UnityEngine;

// the story's screen glitches, separate from DeathGlitch so each moment has its own look
public class StoryGlitch : MonoBehaviour
{
    [SerializeField] private AnalogGlitchController analog;
    [SerializeField] private DigitalGlitchController digital;
    [SerializeField, Range(0f, 1f)] private float digitalStrength = 0.6f;
    [SerializeField, Range(0f, 1f)] private float driftStrength = 0.8f;

    private float digitalEnd;
    private float digitalLength;
    private float driftStart;
    private float driftLength;
    private bool drifting;

    // chunky block corruption that hits hard then fades, for the family
    public void DigitalPulse(float seconds)
    {
        digitalLength = Mathf.Max(seconds, 0.01f);
        digitalEnd = Time.unscaledTime + digitalLength;
        digital.enabled = true;
    }

    // colour drift that builds up and holds, for the pull into the code
    public void ColourDrift(float seconds)
    {
        driftLength = Mathf.Max(seconds, 0.01f);
        driftStart = Time.unscaledTime;
        drifting = true;
    }

    private void Update()
    {
        if (digital.enabled)
        {
            float t = Mathf.Clamp01((digitalEnd - Time.unscaledTime) / digitalLength);
            digital.Intensity = digitalStrength * t * t;
            if (t <= 0f) digital.enabled = false;
        }

        if (drifting)
            analog.ColorDrift = driftStrength * Mathf.Clamp01((Time.unscaledTime - driftStart) / driftLength);
    }
}
