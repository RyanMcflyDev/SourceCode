using UnityEngine;

// scrambles into its text when it appears, then glitches now and then, used on code vision letters and binary
[RequireComponent(typeof(TextMesh))]
public class CodeText : MonoBehaviour
{
    [SerializeField] private string glyphs = "01#%&$@?";
    [SerializeField] private float decodeTime = 0.4f;
    [SerializeField] private float stepTime = 0.05f;
    [SerializeField] private Vector2 glitchGap = new Vector2(2f, 5f);
    [SerializeField] private float glitchTime = 0.08f;
    [SerializeField] private float jitter = 0.05f;

    private TextMesh textMesh;
    private string text;
    private Vector3 startPosition;
    private float decodeEnd;
    private float nextStep;
    private float nextGlitch;
    private float glitchEnd;

    private void Awake()
    {
        textMesh = GetComponent<TextMesh>();
        text = textMesh.text;
        startPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        decodeEnd = Time.time + decodeTime;
        // so the real text comes back as soon as the decode finishes
        glitchEnd = decodeEnd;
        nextStep = 0f;
        nextGlitch = decodeEnd + Random.Range(glitchGap.x, glitchGap.y);
    }

    // back to normal so it reappears clean
    private void OnDisable()
    {
        if (textMesh != null) textMesh.text = text;
        transform.localPosition = startPosition;
    }

    private void Update()
    {
        if (Time.time < decodeEnd)
        {
            if (Time.time >= nextStep)
            {
                textMesh.text = Scramble();
                nextStep = Time.time + stepTime;
            }
            return;
        }

        if (Time.time >= nextGlitch)
        {
            textMesh.text = Scramble();
            transform.localPosition = startPosition + (Vector3)Random.insideUnitCircle * jitter;
            glitchEnd = Time.time + glitchTime;
            nextGlitch = Time.time + Random.Range(glitchGap.x, glitchGap.y);
        }
        else if (glitchEnd > 0f && Time.time >= glitchEnd)
        {
            textMesh.text = text;
            transform.localPosition = startPosition;
            glitchEnd = 0f;
        }
    }

    // same shape as the real text, line breaks and spaces kept
    private string Scramble()
    {
        char[] chars = text.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            if (chars[i] != '\n' && chars[i] != ' ') chars[i] = glyphs[Random.Range(0, glyphs.Length)];
        }
        return new string(chars);
    }
}
