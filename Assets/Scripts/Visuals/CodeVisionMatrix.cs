using UnityEngine;

// fades the matrix shader pack in and out with code vision
// the pack's own MatrixControl reads the old input system, so this sets the same shader globals instead
public class CodeVisionMatrix : MonoBehaviour
{
    [SerializeField] private Texture2D noise;
    [ColorUsage(true, true)]
    [SerializeField] private Color symbolsColor = Color.green;
    // the pack goes from -1 (off) to 1 (object fully black under the symbols), lower shows more of the object
    [Range(-1f, 1f)]
    [SerializeField] private float onAmount = 1f;
    [SerializeField] private float fadeSpeed = 3f;
    // for scenes without code vision where something wears the matrix look anyway, like byte in glitch's level 1 talk
    [SerializeField] private bool startOn;

    private float current = -1f;
    private float target = -1f;

    private void OnEnable()
    {
        GameEvents.CodeVisionChanged += OnCodeVisionChanged;
        if (startOn) current = target = onAmount;
        Shader.SetGlobalColor("_SymbolsColor", symbolsColor);
        Shader.SetGlobalTexture("_MatrixMask", noise);
        Shader.SetGlobalFloat("_Matrix", current);
    }

    private void OnDisable() => GameEvents.CodeVisionChanged -= OnCodeVisionChanged;

    private void OnCodeVisionChanged(bool codeVisionOn)
    {
        // objects only switch to the matrix material while code vision is on, so each use fades in from the start
        if (codeVisionOn) current = -1f;
        target = codeVisionOn ? onAmount : -1f;
    }

    private void Update()
    {
        if (current == target) return;

        current = Mathf.MoveTowards(current, target, fadeSpeed * Time.deltaTime);
        Shader.SetGlobalFloat("_Matrix", current);
    }
}
