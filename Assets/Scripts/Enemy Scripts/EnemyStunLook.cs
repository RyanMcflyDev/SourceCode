using UnityEngine;

// shows the enemy is stunned after the first swap: the model shakes and flashes until the stun wears off
public class EnemyStunLook : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyAI enemy;
    [SerializeField] private Transform model;

    [Header("Look")]
    [SerializeField] private float shakeAmount = 0.08f;
    [ColorUsage(false, true)]
    [SerializeField] private Color flashColour = new Color(4f, 0.4f, 0.4f);
    [SerializeField] private float flashSpeed = 14f;

    private static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

    private Renderer modelRenderer;
    private EnemyGlitch idleGlitch;
    private MaterialPropertyBlock block;
    private Vector3 modelPosition;
    private Vector3 modelScale;
    private bool wasStunned;

    private void Awake()
    {
        if (enemy == null) enemy = GetComponent<EnemyAI>();
        modelRenderer = model.GetComponentInChildren<Renderer>();
        idleGlitch = model.GetComponent<EnemyGlitch>();
        modelPosition = model.localPosition;
        modelScale = model.localScale;
        block = new MaterialPropertyBlock();
    }

    private void Update()
    {
        bool stunned = enemy.CurrentState == EnemyAI.AIState.Confused;

        if (stunned)
        {
            // the idle glitch would fight the shake for the same transform
            if (!wasStunned && idleGlitch != null) idleGlitch.enabled = false;

            model.localPosition = modelPosition + Random.insideUnitSphere * shakeAmount;
            float pulse = (Mathf.Sin(Time.time * flashSpeed) + 1f) * 0.5f;
            block.SetColor(EmissionId, flashColour * pulse);
            modelRenderer.SetPropertyBlock(block);
        }
        else if (wasStunned)
        {
            StopStun();
        }

        wasStunned = stunned;
    }

    // dying or the room resetting mid stun
    private void OnDisable()
    {
        if (wasStunned) StopStun();
        wasStunned = false;
    }

    private void StopStun()
    {
        model.localPosition = modelPosition;
        model.localScale = modelScale;
        modelRenderer.SetPropertyBlock(null);
        if (idleGlitch != null) idleGlitch.enabled = true;
    }
}
