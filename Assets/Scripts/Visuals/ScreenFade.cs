using UnityEngine;

// fades the screen to and from black with a quad in front of the camera
public class ScreenFade : MonoBehaviour
{
    [SerializeField] private Renderer cover;
    [SerializeField] private bool startBlack;
    // above 0 fades in from black on start, for arriving in a level
    [SerializeField] private float fadeInOnStart;

    private Material material;
    private float alpha;
    private float target;
    private float speed;

    private void Awake()
    {
        material = cover.material;
        SetAlpha(startBlack ? 1f : 0f);
        target = alpha;
    }

    private void Start()
    {
        if (fadeInOnStart > 0f) FadeIn(fadeInOnStart);
    }

    public void FadeOut(float seconds) => FadeTo(1f, seconds);
    public void FadeIn(float seconds) => FadeTo(0f, seconds);

    private void FadeTo(float value, float seconds)
    {
        target = value;
        speed = 1f / Mathf.Max(seconds, 0.01f);
    }

    private void Update()
    {
        if (alpha != target) SetAlpha(Mathf.MoveTowards(alpha, target, speed * Time.unscaledDeltaTime));
    }

    private void SetAlpha(float value)
    {
        alpha = value;
        Color colour = material.GetColor("_BaseColor");
        colour.a = value;
        material.SetColor("_BaseColor", colour);
        cover.enabled = value > 0f;
    }
}
