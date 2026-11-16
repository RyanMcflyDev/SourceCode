using UnityEngine;

// corrupted townsfolk glitch every so often: they snap out of shape, their outfit colours scramble and the walk stutters
[RequireComponent(typeof(Animator))]
public class EnemyGlitch : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float minWait = 0.6f;
    [SerializeField] private float maxWait = 2.5f;
    [SerializeField] private float minLength = 0.06f;
    [SerializeField] private float maxLength = 0.2f;

    [Header("Strength")]
    [SerializeField] private float positionAmount = 0.12f;
    [SerializeField] private float scaleAmount = 0.15f;
    // only some glitches flash red, the rest just stutter and scramble
    [Range(0f, 1f)]
    [SerializeField] private float redChance = 0.3f;
    [ColorUsage(false, true)]
    [SerializeField] private Color glitchColour = new Color(2.5f, 0.1f, 0.2f);

    private static readonly int BaseMapStId = Shader.PropertyToID("_BaseMap_ST");
    private static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

    private Renderer modelRenderer;
    private Animator animator;
    private MaterialPropertyBlock block;
    private Vector3 startPosition;
    private Vector3 startScale;
    private float nextGlitch;
    private float glitchEnd;
    private bool glitching;
    private bool flashRed;

    private void Awake()
    {
        modelRenderer = GetComponentInChildren<Renderer>();
        animator = GetComponent<Animator>();
        block = new MaterialPropertyBlock();
        startPosition = transform.localPosition;
        startScale = transform.localScale;
    }

    private void OnEnable() => nextGlitch = Time.time + Random.Range(minWait, maxWait);

    // the stun look switches this off, so it has to leave the model clean
    private void OnDisable()
    {
        if (glitching) StopGlitch();
    }

    private void Update()
    {
        if (!glitching && Time.time >= nextGlitch) StartGlitch();
        if (!glitching) return;

        if (Time.time >= glitchEnd)
        {
            StopGlitch();
            return;
        }

        // a new random offset every frame while it lasts
        transform.localPosition = startPosition + Random.insideUnitSphere * positionAmount;
        transform.localScale = Vector3.Scale(startScale, new Vector3(1f + Random.Range(-scaleAmount, scaleAmount), 1f + Random.Range(-scaleAmount, scaleAmount), 1f));

        // shifting the atlas jumbles the outfit colours
        block.SetVector(BaseMapStId, new Vector4(1f, 1f, Random.value, Random.value));
        if (flashRed) block.SetColor(EmissionId, Random.value > 0.5f ? glitchColour : Color.black);
        modelRenderer.SetPropertyBlock(block);
    }

    private void StartGlitch()
    {
        glitching = true;
        flashRed = Random.value < redChance;
        glitchEnd = Time.time + Random.Range(minLength, maxLength);
        // the pose freezes while it glitches, so the walk stutters
        animator.speed = 0f;
    }

    private void StopGlitch()
    {
        glitching = false;
        nextGlitch = Time.time + Random.Range(minWait, maxWait);
        transform.localPosition = startPosition;
        transform.localScale = startScale;
        animator.speed = 1f;
        // cleared so a non red glitch next time doesn't keep the old colour
        block.Clear();
        modelRenderer.SetPropertyBlock(null);
    }
}
