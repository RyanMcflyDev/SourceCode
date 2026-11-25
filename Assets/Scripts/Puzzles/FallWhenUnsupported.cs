using UnityEngine;

// a swapped object with nothing under it drops, e.g. a box swapped to where byte was falling into a pit
// no rigidbody so enemies can't shove boxes around, it falls straight down until it lands or drops out of sight
[RequireComponent(typeof(Collider))]
public class FallWhenUnsupported : MonoBehaviour, ITeleportable, IResettable
{
    [SerializeField] private float gravity = 20f;
    // switched off after falling this far, the room reset brings it back
    [SerializeField] private float maxFallDistance = 15f;

    // the ray starts this far above the bottom of the collider so it doesn't start inside the floor
    private const float StartAbove = 0.1f;
    private const float GroundedDistance = 0.1f;

    private Collider body;
    private bool isFalling;
    private float speed;
    private float fallen;

    private void Awake() => body = GetComponent<Collider>();

    public void OnTeleported()
    {
        if (FindGround(GroundedDistance, out _)) return;

        isFalling = true;
        speed = 0f;
        fallen = 0f;
    }

    private void Update()
    {
        if (!isFalling) return;

        speed += gravity * Time.deltaTime;
        float step = speed * Time.deltaTime;

        // landed on something, e.g. the trap door shaft floor
        if (FindGround(step, out float distance))
        {
            transform.position += Vector3.down * distance;
            isFalling = false;
            return;
        }

        transform.position += Vector3.down * step;
        fallen += step;

        if (fallen >= maxFallDistance)
        {
            isFalling = false;
            gameObject.SetActive(false);
        }
    }

    public void ResetState() => isFalling = false;

    // nearest solid thing below the bottom of the collider within range, skipping this object
    private bool FindGround(float range, out float distance)
    {
        Bounds bounds = body.bounds;
        Vector3 start = new Vector3(bounds.center.x, bounds.min.y + StartAbove, bounds.center.z);
        distance = float.MaxValue;

        foreach (RaycastHit hit in Physics.RaycastAll(start, Vector3.down, range + StartAbove, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform.IsChildOf(transform)) continue;
            distance = Mathf.Min(distance, hit.distance - StartAbove);
        }

        return distance != float.MaxValue;
    }
}
