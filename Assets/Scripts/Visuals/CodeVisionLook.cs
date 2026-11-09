using UnityEngine;

// swaps material when code vision changes, replace with the shader once real assets are in
[RequireComponent(typeof(Renderer))]
public class CodeVisionLook : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material codeVisionMaterial;

    private Renderer targetRenderer;

    private void Awake()
    {
        targetRenderer = GetComponent<Renderer>();
        // kit art keeps its own material when code vision is off
        if (normalMaterial == null)
            normalMaterial = targetRenderer.sharedMaterial;
    }

    private void OnEnable()
    {
        GameEvents.CodeVisionChanged += Apply;
        Apply(GameEvents.CodeVisionActive);
    }

    private void OnDisable() => GameEvents.CodeVisionChanged -= Apply;

    private void Apply(bool codeVisionOn)
    {
        Material material = codeVisionOn ? codeVisionMaterial : normalMaterial;
        if (material != null) targetRenderer.sharedMaterial = material;
    }
}
