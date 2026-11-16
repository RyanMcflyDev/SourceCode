using UnityEngine;
using UnityEngine.AI;

// drives the walk blend from the navmesh agent, same parameters as ByteAnimator so it can use a copy of his controller
[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;

    [Header("Locomotion")]
    // ground speed of the fastest clip in the blend tree, keep it the same as that clip's threshold
    [SerializeField] private float topClipSpeed = 1.37f;
    [SerializeField] private float maxPlaybackRate = 3f;
    [SerializeField] private float speedDamping = 0.08f;

    [Header("Facing")]
    // the model faces where it's actually moving so the legs dont slide, the root keeps the agent's facing for the sight cone
    [SerializeField] private float turnSpeed = 900f;
    [SerializeField] private float minTurnSpeed = 0.2f;

    private static readonly int SpeedId = Animator.StringToHash("Speed");
    private static readonly int WalkRateId = Animator.StringToHash("WalkRate");

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (agent == null) agent = GetComponentInParent<NavMeshAgent>();
    }

    private void Update()
    {
        // the agent is switched off when the enemy dies, asking it for velocity then logs errors
        Vector3 velocity = Vector3.zero;
        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            velocity = agent.velocity;
            velocity.y = 0f;
        }
        float speed = velocity.magnitude;

        animator.SetFloat(SpeedId, speed, speedDamping, Time.deltaTime);

        // faster than the top clip, play it faster so the feet dont slide
        float rate = speed > topClipSpeed ? speed / topClipSpeed : 1f;
        animator.SetFloat(WalkRateId, Mathf.Min(rate, maxPlaybackRate));

        if (speed > minTurnSpeed)
        {
            Quaternion target = Quaternion.LookRotation(velocity);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * Time.deltaTime);
        }
        else
        {
            // standing still, line back up with the body
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, Quaternion.identity, turnSpeed * Time.deltaTime);
        }
    }
}
