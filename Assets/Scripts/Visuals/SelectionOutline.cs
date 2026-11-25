using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// draws an outline round whatever byte has selected, by drawing copies of the target's meshes with the outline material
// the copies live outside the target so code vision, dissolve and glitch material swaps never touch them
// an OutlineStyle on the target changes its width and colour, otherwise the material's own values are used
public class SelectionOutline : MonoBehaviour
{
    [SerializeField] private Material outlineMaterial;

    private static readonly int CentreId = Shader.PropertyToID("_OutlineCentre");
    private static readonly int WidthId = Shader.PropertyToID("_Width");
    private static readonly int ColourId = Shader.PropertyToID("_Color");

    private readonly List<Renderer> sources = new List<Renderer>();
    private readonly List<Renderer> copies = new List<Renderer>();
    private MaterialPropertyBlock block;
    private OutlineStyle style;

    private void Awake() => block = new MaterialPropertyBlock();

    private void OnEnable() => GameEvents.SelectionChanged += OnSelectionChanged;

    private void OnDisable()
    {
        GameEvents.SelectionChanged -= OnSelectionChanged;
        Clear();
    }

    private void OnSelectionChanged(Transform target)
    {
        Clear();
        if (target == null) return;

        style = target.GetComponentInChildren<OutlineStyle>();

        foreach (Renderer source in target.GetComponentsInChildren<Renderer>())
        {
            // pack models hold every outfit with only one switched on
            if (!source.enabled || !source.gameObject.activeInHierarchy) continue;

            Renderer copy = MakeCopy(source);
            if (copy == null) continue;

            sources.Add(source);
            copies.Add(copy);
        }
    }

    // after the target has moved and animated this frame
    private void LateUpdate()
    {
        for (int i = 0; i < copies.Count; i++)
        {
            Renderer source = sources[i];
            Renderer copy = copies[i];

            bool visible = source != null && source.enabled && source.gameObject.activeInHierarchy;
            copy.enabled = visible;
            if (!visible) continue;

            Transform from = source.transform;
            copy.transform.SetPositionAndRotation(from.position, from.rotation);
            copy.transform.localScale = from.lossyScale;

            copy.GetPropertyBlock(block);
            block.SetVector(CentreId, source.bounds.center);
            if (style != null)
            {
                block.SetFloat(WidthId, style.Width);
                if (style.OverrideColour) block.SetColor(ColourId, style.Colour);
            }
            copy.SetPropertyBlock(block);
        }
    }

    private Renderer MakeCopy(Renderer source)
    {
        if (source is SkinnedMeshRenderer skinned)
        {
            if (skinned.sharedMesh == null) return null;

            // shares the source's bones, so it follows the animation without any extra work
            var copy = NewCopyObject(source).AddComponent<SkinnedMeshRenderer>();
            copy.sharedMesh = skinned.sharedMesh;
            copy.bones = skinned.bones;
            copy.rootBone = skinned.rootBone;
            copy.localBounds = skinned.localBounds;
            Setup(copy, skinned.sharedMesh.subMeshCount);
            return copy;
        }

        // text meshes have a MeshRenderer but no MeshFilter, they're skipped here
        if (source is MeshRenderer && source.TryGetComponent(out MeshFilter filter) && filter.sharedMesh != null)
        {
            GameObject copyObject = NewCopyObject(source);
            copyObject.AddComponent<MeshFilter>().sharedMesh = filter.sharedMesh;
            var copy = copyObject.AddComponent<MeshRenderer>();
            Setup(copy, filter.sharedMesh.subMeshCount);
            return copy;
        }

        return null;
    }

    private static GameObject NewCopyObject(Renderer source)
    {
        var copyObject = new GameObject("Outline " + source.name);
        copyObject.layer = source.gameObject.layer;
        return copyObject;
    }

    private void Setup(Renderer copy, int subMeshCount)
    {
        var materials = new Material[Mathf.Max(1, subMeshCount)];
        for (int i = 0; i < materials.Length; i++) materials[i] = outlineMaterial;
        copy.sharedMaterials = materials;
        copy.shadowCastingMode = ShadowCastingMode.Off;
        copy.receiveShadows = false;
        copy.lightProbeUsage = LightProbeUsage.Off;
        copy.reflectionProbeUsage = ReflectionProbeUsage.Off;
    }

    private void Clear()
    {
        foreach (Renderer copy in copies)
            if (copy != null) Destroy(copy.gameObject);

        sources.Clear();
        copies.Clear();
        style = null;
    }
}
