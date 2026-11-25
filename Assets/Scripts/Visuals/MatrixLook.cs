using UnityEngine;

// puts renderers in a matrix material and fades the matrix in, then fades it out before putting their normal materials back
// for glitch's level 1 talk (byte and the crumbling wall), game creator calls On and Off with Invoke Method
// the fade is the scene's CodeVisionMatrix, which is shared, so everything wearing the matrix fades together
// don't use this in a level where code vision is also running
public class MatrixLook : MonoBehaviour
{
    [SerializeField] private Renderer[] targetRenderers;
    [SerializeField] private Material matrixMaterial;
    [SerializeField] private CodeVisionMatrix matrix;

    private Material[][] normalMaterials;
    private bool waitingToRestore;

    private void Awake()
    {
        normalMaterials = new Material[targetRenderers.Length][];
        for (int i = 0; i < targetRenderers.Length; i++)
            normalMaterials[i] = targetRenderers[i].sharedMaterials;
    }

    // the matrix starts fully off, which looks like the normal material, so the swap doesn't pop
    public void On()
    {
        waitingToRestore = false;
        foreach (Renderer target in targetRenderers)
        {
            var materials = new Material[target.sharedMaterials.Length];
            for (int i = 0; i < materials.Length; i++) materials[i] = matrixMaterial;
            target.sharedMaterials = materials;
        }
        matrix.FadeIn();
    }

    public void Off()
    {
        matrix.FadeOut();
        waitingToRestore = true;
    }

    private void Update()
    {
        if (!waitingToRestore || !matrix.IsOff) return;

        for (int i = 0; i < targetRenderers.Length; i++)
            targetRenderers[i].sharedMaterials = normalMaterials[i];
        waitingToRestore = false;
    }
}
