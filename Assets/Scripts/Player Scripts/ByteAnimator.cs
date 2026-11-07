using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ByteAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController _controller;

    [Header("Locomotion")]
    // ground speed of the fastest clip in the blend tree, keep it the same as that clip's threshold
    [SerializeField] private float _topClipSpeed = 1.37f;
    [SerializeField] private float _maxPlaybackRate = 3f;
    [SerializeField] private float _speedDamping = 0.08f;

    private static readonly int SpeedId = Animator.StringToHash("Speed");
    private static readonly int WalkRateId = Animator.StringToHash("WalkRate");

    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        if (_controller == null)
            _controller = GetComponentInParent<CharacterController>();
    }

    private void Update()
    {
        // controller velocity ignores swap teleports, so no speed spike after a swap
        Vector3 velocity = _controller.velocity;
        velocity.y = 0f;
        float speed = velocity.magnitude;

        _animator.SetFloat(SpeedId, speed, _speedDamping, Time.deltaTime);

        // faster than the top clip, play it faster so the feet dont slide
        float rate = speed > _topClipSpeed ? speed / _topClipSpeed : 1f;
        _animator.SetFloat(WalkRateId, Mathf.Min(rate, _maxPlaybackRate));
    }
}
