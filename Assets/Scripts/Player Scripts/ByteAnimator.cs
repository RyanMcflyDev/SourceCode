using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ByteAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController controller;

    [Header("Locomotion")]
    // ground speed of the fastest clip in the blend tree, keep it the same as that clip's threshold
    [SerializeField] private float topClipSpeed = 1.37f;
    [SerializeField] private float maxPlaybackRate = 3f;
    [SerializeField] private float speedDamping = 0.08f;

    private static readonly int SpeedId = Animator.StringToHash("Speed");
    private static readonly int WalkRateId = Animator.StringToHash("WalkRate");

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (controller == null)
            controller = GetComponentInParent<CharacterController>();
    }

    private void Update()
    {
        // controller velocity ignores swap teleports, so no speed spike after a swap
        Vector3 velocity = controller.velocity;
        velocity.y = 0f;
        float speed = velocity.magnitude;

        animator.SetFloat(SpeedId, speed, speedDamping, Time.deltaTime);

        // faster than the top clip, play it faster so the feet dont slide
        float rate = speed > topClipSpeed ? speed / topClipSpeed : 1f;
        animator.SetFloat(WalkRateId, Mathf.Min(rate, maxPlaybackRate));
    }
}
