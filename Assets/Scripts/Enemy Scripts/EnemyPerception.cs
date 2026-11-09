using UnityEngine;

public class EnemyPerception : MonoBehaviour
{
    [Header("Field of View")]
    [SerializeField, Range(0f, 180f)] private float innerAngle = 40f;
    [SerializeField, Range(0f, 180f)] private float outerAngle = 80f;
    [SerializeField, Min(0f)] private float detectionRange = 10f;
    [SerializeField, Min(0f)] private float peripheralRange = 4f;

    [Header("Line of Sight")]
    // include byte's layer and anything that should block sight (walls etc)
    [SerializeField] private LayerMask sightBlockMask = -1;

    private const int ArcSegments = 20;

    public bool CanSeePlayer(Transform origin, Transform player, bool alerted = false)
    {
        if (player == null) return false;

        Vector3 toPlayer = player.position - origin.position;
        float distanceToPlayer = toPlayer.magnitude;
        if (distanceToPlayer < Mathf.Epsilon) return true;

        Vector3 dirToPlayer = toPlayer / distanceToPlayer;
        float angleToPlayer = Vector3.Angle(origin.forward, dirToPlayer);

        // once alerted it tracks byte all round, the cone only matters for spotting him
        if (!alerted && angleToPlayer > outerAngle) return false;

        // full range inside the inner cone, shrinking to peripheral range at the outer edge
        float effectiveRange = alerted || angleToPlayer <= innerAngle
            ? detectionRange
            : Mathf.Lerp(detectionRange, peripheralRange, Mathf.InverseLerp(innerAngle, outerAngle, angleToPlayer));

        if (distanceToPlayer > effectiveRange) return false;

        // ignore triggers so detection zones don't block sight
        if (!Physics.Raycast(origin.position, dirToPlayer, out RaycastHit hit, effectiveRange, sightBlockMask, QueryTriggerInteraction.Ignore))
            return false;

        return hit.collider.CompareTag("Player");
    }

    private void OnValidate()
    {
        // stop the lerp inverting if these are set the wrong way round in the inspector
        innerAngle = Mathf.Min(innerAngle, outerAngle);
        peripheralRange = Mathf.Min(peripheralRange, detectionRange);
    }

    // only draws when the enemy is selected and gizmos are on in the scene view
    private void OnDrawGizmosSelected()
    {
        DrawConeGizmo(Color.green, innerAngle, detectionRange);
        DrawConeGizmo(new Color(1f, 0.5f, 0f), outerAngle, peripheralRange);
    }

    private void DrawConeGizmo(Color color, float halfAngle, float range)
    {
        Gizmos.color = color;
        Gizmos.DrawRay(transform.position, transform.forward * range);
        Gizmos.DrawRay(transform.position, Quaternion.Euler(0f, -halfAngle, 0f) * transform.forward * range);
        Gizmos.DrawRay(transform.position, Quaternion.Euler(0f, halfAngle, 0f) * transform.forward * range);

        float step = halfAngle * 2f / ArcSegments;
        Vector3 previous = transform.position + Quaternion.Euler(0f, -halfAngle, 0f) * transform.forward * range;

        for (int i = 1; i <= ArcSegments; i++)
        {
            Vector3 next = transform.position + Quaternion.Euler(0f, -halfAngle + step * i, 0f) * transform.forward * range;
            Gizmos.DrawLine(previous, next);
            previous = next;
        }
    }
}
