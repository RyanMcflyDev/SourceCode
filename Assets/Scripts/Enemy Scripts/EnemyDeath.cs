using UnityEngine;
using UnityEngine.AI;

public class EnemyDeath : MonoBehaviour, IKillable, IResettable
{
    [Header("Dissolve")]
    // leave empty and the enemy just vanishes
    [SerializeField] private Material dissolveMaterial;
    [SerializeField] private float dissolveTime = 1f;
    // how far the shader's noise spreads the edge, so the sweep ends low enough to clear the feet
    [SerializeField] private float noiseHeight = 1f;
    // reference names from the dissolve shader graph, it uses generated ones
    [SerializeField] private string cutoffProperty = "Vector1_CFBBCBA";
    [SerializeField] private string colorProperty = "Color_87DA54D1";

    private Rigidbody body;
    private Renderer meshRenderer;
    private Collider[] colliders;
    private NavMeshAgent agent;
    private Material normalMaterial;
    private Material dissolveInstance;
    private float dissolveTimer;
    private float cutoffTop;
    private float cutoffBottom;
    private bool dissolving;

    public bool IsDead { get; private set; }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        meshRenderer = GetComponentInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();
        agent = GetComponent<NavMeshAgent>();
        normalMaterial = meshRenderer.sharedMaterial;

        if (dissolveMaterial != null)
        {
            // its own copy so each enemy dissolves separately, tinted to match the enemy
            dissolveInstance = new Material(dissolveMaterial);
            if (normalMaterial.HasProperty("_BaseColor"))
                dissolveInstance.SetColor(colorProperty, normalMaterial.GetColor("_BaseColor"));
            // textured dissolve materials keep the model's look, the enemy one has no texture slot
            if (dissolveInstance.HasProperty("_BaseMap") && normalMaterial.HasProperty("_BaseMap"))
                dissolveInstance.SetTexture("_BaseMap", normalMaterial.GetTexture("_BaseMap"));
        }
    }

    // stops counting for gameplay straight away, dissolves, then turns off (not destroyed so the room reset can bring it back)
    public void Kill(string cause)
    {
        if (IsDead) return;
        IsDead = true;

        Debug.Log($"[EnemyDeath] {gameObject.name} killed by {cause}");

        // falling enemies are already out of sight in the pit, just remove them
        bool falling = body != null && !body.isKinematic;
        if (dissolveInstance == null || falling)
        {
            gameObject.SetActive(false);
            return;
        }

        foreach (Collider c in colliders) c.enabled = false;
        if (agent != null) agent.enabled = false;

        // the shader cuts by world height, sweep from the top of the enemy to below its feet
        // worked out from the transform as renderer bounds can lag a frame behind a swap teleport
        float centre;
        float halfHeight;
        if (meshRenderer is SkinnedMeshRenderer)
        {
            // skinned bounds are along the root bone (sideways on the people models), world bounds give the real height
            centre = meshRenderer.bounds.center.y;
            halfHeight = meshRenderer.bounds.extents.y;
        }
        else
        {
            Transform model = meshRenderer.transform;
            centre = model.TransformPoint(meshRenderer.localBounds.center).y;
            halfHeight = meshRenderer.localBounds.extents.y * model.lossyScale.y;
        }
        cutoffTop = centre + halfHeight;
        cutoffBottom = centre - halfHeight - noiseHeight;
        dissolveTimer = 0f;
        dissolving = true;
        meshRenderer.sharedMaterial = dissolveInstance;
    }

    private void Update()
    {
        if (!dissolving) return;

        dissolveTimer += Time.deltaTime;
        float t = dissolveTimer / dissolveTime;
        dissolveInstance.SetFloat(cutoffProperty, Mathf.Lerp(cutoffTop, cutoffBottom, t));

        if (t >= 1f)
        {
            dissolving = false;
            gameObject.SetActive(false);
        }
    }

    public void ResetState()
    {
        IsDead = false;
        dissolving = false;
        meshRenderer.sharedMaterial = normalMaterial;
        foreach (Collider c in colliders) c.enabled = true;

        if (body == null) return;

        // stop any fall speed then go back to being moved by the navmesh agent
        if (!body.isKinematic)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
        body.isKinematic = true;
    }
}
