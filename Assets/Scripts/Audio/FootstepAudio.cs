using UnityEngine;

// plays a footstep each time one of byte's feet comes back down to the floor
[RequireComponent(typeof(AudioSource), typeof(Animator))]
public class FootstepAudio : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController _controller;
    [SerializeField] private FootstepSet _defaultFootsteps;

    [Header("Ground Check")]
    [SerializeField] private LayerMask _groundLayers;
    [SerializeField] private float _checkDistance = 1.5f;

    [Header("Step Detection")]
    // a foot lands when it gets this close to its lowest point, and has to lift this high before the next step
    [SerializeField] private float _landHeight = 0.03f;
    [SerializeField] private float _liftHeight = 0.06f;
    // lets the lowest point creep back up so stairs and ledges don't throw it off
    [SerializeField] private float _settleSpeed = 0.05f;

    [Header("Feel")]
    [SerializeField] private float _runSpeed = 2f;
    [SerializeField] private float _minSpeed = 0.2f;
    [SerializeField] private float _minGap = 0.08f;
    [SerializeField] private float _volume = 0.6f;
    [SerializeField] private float _pitchVariation = 0.08f;
    [SerializeField] private float _volumeVariation = 0.1f;

    private class Foot
    {
        public Transform Bone;
        public float Lowest = float.MaxValue;
        public bool Lifted;
    }

    private readonly Foot _left = new Foot();
    private readonly Foot _right = new Foot();

    private AudioSource _source;
    private AudioClip _lastClip;
    private float _nextStepTime;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        if (_controller == null)
            _controller = GetComponentInParent<CharacterController>();

        Animator animator = GetComponent<Animator>();
        _left.Bone = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        _right.Bone = animator.GetBoneTransform(HumanBodyBones.RightFoot);
    }

    // late update so the feet are read after animation and foot ik have moved them
    private void LateUpdate()
    {
        float floor = _controller.bounds.min.y;
        CheckFoot(_left, floor);
        CheckFoot(_right, floor);
    }

    private void CheckFoot(Foot foot, float floor)
    {
        float height = foot.Bone.position.y - floor;
        foot.Lowest = Mathf.Min(foot.Lowest + _settleSpeed * Time.deltaTime, height);

        if (height > foot.Lowest + _liftHeight)
        {
            foot.Lifted = true;
        }
        else if (foot.Lifted && height < foot.Lowest + _landHeight)
        {
            foot.Lifted = false;
            PlayStep();
        }
    }

    private void PlayStep()
    {
        if (Time.time < _nextStepTime) return;

        // idle sway and swap teleports move the feet too, only play when actually walking
        Vector3 velocity = _controller.velocity;
        velocity.y = 0f;
        float speed = velocity.magnitude;
        if (speed < _minSpeed || !_controller.isGrounded) return;

        FootstepSet footsteps = GetFootsteps();
        if (footsteps == null) return;

        bool running = speed >= _runSpeed && footsteps.RunClips.Length > 0;
        AudioClip clip = PickClip(running ? footsteps.RunClips : footsteps.WalkClips);
        if (clip == null) return;

        _source.pitch = 1f + Random.Range(-_pitchVariation, _pitchVariation);
        _source.PlayOneShot(clip, _volume * (1f + Random.Range(-_volumeVariation, _volumeVariation)));
        _nextStepTime = Time.time + _minGap;
    }

    private FootstepSet GetFootsteps()
    {
        Bounds body = _controller.bounds;
        Vector3 feet = new Vector3(body.center.x, body.min.y, body.center.z);

        FootstepSet zone = FootstepSurface.FindZone(feet);
        if (zone != null) return zone;

        if (Physics.Raycast(feet + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, _checkDistance, _groundLayers, QueryTriggerInteraction.Ignore))
        {
            var surface = hit.collider.GetComponentInParent<FootstepSurface>();
            if (surface != null && surface.Footsteps != null) return surface.Footsteps;
        }

        return _defaultFootsteps;
    }

    // random clip, never the same one twice in a row
    private AudioClip PickClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        if (clips.Length == 1) return clips[0];

        AudioClip clip;
        do clip = clips[Random.Range(0, clips.Length)];
        while (clip == _lastClip);

        _lastClip = clip;
        return clip;
    }
}
