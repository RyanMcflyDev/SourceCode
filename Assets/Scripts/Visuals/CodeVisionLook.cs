using UnityEngine;

// swaps material when code vision changes, replace with the shader once real assets are in
[RequireComponent(typeof(Renderer))]
public class CodeVisionLook : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material _normalMaterial;
    [SerializeField] private Material _codeVisionMaterial;

    private Renderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    private void OnEnable()
    {
        GameEvents.CodeVisionChanged += Apply;
        Apply(GameEvents.CodeVisionActive);
    }

    private void OnDisable() => GameEvents.CodeVisionChanged -= Apply;

    private void Apply(bool codeVisionOn)
    {
        Material material = codeVisionOn ? _codeVisionMaterial : _normalMaterial;
        if (material != null) _renderer.sharedMaterial = material;
    }
}
