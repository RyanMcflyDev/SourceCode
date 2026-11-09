using UnityEngine;

// plays a footstep each time one of byte's feet comes back down to the floor
[RequireComponent(typeof(AudioSource), typeof(Animator))]
public class FootstepAudio : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private FootstepSet defaultFootsteps;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private float checkDistance = 1.5f;

    [Header("Step Detection")]
    // a foot lands when it gets this close to its lowest point, and has to lift this high before the next step
    [SerializeField] private float landHeight = 0.03f;
    [SerializeField] private float liftHeight = 0.06f;
    // lets the lowest point creep back up so stairs and ledges don't throw it off
    [SerializeField] private float settleSpeed = 0.05f;

    [Header("Feel")]
    [SerializeField] private float runSpeed = 2f;
    [SerializeField] private float minSpeed = 0.2f;
    [SerializeField] private float minGap = 0.08f;
    [SerializeField] private float volume = 0.6f;
    [SerializeField] private float pitchVariation = 0.08f;
    [SerializeField] private float volumeVariation = 0.1f;

    private class Foot
    {
        public Transform Bone;
        public float Lowest = float.MaxValue;
        public bool Lifted;
    }

    private readonly Foot leftFoot = new Foot();
    private readonly Foot rightFoot = new Foot();

    private AudioSource source;
    private AudioClip lastClip;
    private float nextStepTime;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        if (controller == null)
            controller = GetComponentInParent<CharacterController>();

        Animator animator = GetComponent<Animator>();
        leftFoot.Bone = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        rightFoot.Bone = animator.GetBoneTransform(HumanBodyBones.RightFoot);
    }

    // late update so the feet are read after animation and foot ik have moved them
    private void LateUpdate()
    {
        float floor = controller.bounds.min.y;
        CheckFoot(leftFoot, floor);
        CheckFoot(rightFoot, floor);
    }

    private void CheckFoot(Foot foot, float floor)
    {
        float height = foot.Bone.position.y - floor;
        foot.Lowest = Mathf.Min(foot.Lowest + settleSpeed * Time.deltaTime, height);

        if (height > foot.Lowest + liftHeight)
        {
            foot.Lifted = true;
        }
        else if (foot.Lifted && height < foot.Lowest + landHeight)
        {
            foot.Lifted = false;
            PlayStep();
        }
    }

    private void PlayStep()
    {
        if (Time.time < nextStepTime) return;

        // idle sway and swap teleports move the feet too, only play when actually walking
        Vector3 velocity = controller.velocity;
        velocity.y = 0f;
        float speed = velocity.magnitude;
        if (speed < minSpeed || !controller.isGrounded) return;

        FootstepSet footsteps = GetFootsteps();
        if (footsteps == null) return;

        bool running = speed >= runSpeed && footsteps.RunClips.Length > 0;
        AudioClip clip = PickClip(running ? footsteps.RunClips : footsteps.WalkClips);
        if (clip == null) return;

        source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        source.PlayOneShot(clip, volume * (1f + Random.Range(-volumeVariation, volumeVariation)));
        nextStepTime = Time.time + minGap;
    }

    private FootstepSet GetFootsteps()
    {
        Bounds body = controller.bounds;
        Vector3 feet = new Vector3(body.center.x, body.min.y, body.center.z);

        FootstepSet zoneFootsteps = FootstepSurface.FindZone(feet);
        if (zoneFootsteps != null) return zoneFootsteps;

        if (Physics.Raycast(feet + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, checkDistance, groundLayers, QueryTriggerInteraction.Ignore))
        {
            var surface = hit.collider.GetComponentInParent<FootstepSurface>();
            if (surface != null && surface.Footsteps != null) return surface.Footsteps;
        }

        return defaultFootsteps;
    }

    // random clip, never the same one twice in a row
    private AudioClip PickClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        if (clips.Length == 1) return clips[0];

        AudioClip clip;
        do clip = clips[Random.Range(0, clips.Length)];
        while (clip == lastClip);

        lastClip = clip;
        return clip;
    }
}
