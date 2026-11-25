using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;

public class Swapper : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference swapAction;
    [SerializeField] private InputActionReference pointAction;
    [SerializeField] private InputActionReference selectAction;
    [SerializeField] private InputActionReference cancelAction;

    [Header("References")]
    [SerializeField] private Camera mainCamera;
    // optional, the selection outline does this job now
    [SerializeField] private GameObject markerPrefab;

    [Header("Targeting")]
    [SerializeField] private LayerMask swappableLayers;
    // only these block line of sight, enemies shouldn't
    [SerializeField] private LayerMask sightBlockers;
    // swappables block sight to things behind them, but can still be swapped with themselves
    [SerializeField] private bool swappablesBlockSight = true;
    [SerializeField] private float swapRange = 7f;
    [SerializeField] private float eyeHeight = 1.2f;
    [SerializeField] private float markerHeight = 1.5f;

    [Header("Timing")]
    [SerializeField] private float cooldown = 1f;

    // floor check under the spot the target lands on, from a little above the feet to a little below
    private const float GroundCheckUp = 0.3f;
    private const float GroundCheckDown = 0.5f;
    // anything flatter than this counts as floor, walls don't
    private const float MinFloorNormal = 0.1f;
    // an enemy on solid floor gets pulled this far onto the navmesh, the navmesh stops short of pit edges
    private const float EdgeSnapDistance = 1.5f;
    // the navmesh point has to be on the same floor, so an enemy in the trap door shaft isn't pulled back up
    private const float SameFloorHeight = 0.5f;

    private Transform target;
    private bool hasTarget;
    private GameObject marker;
    private float nextSwapTime;

    private void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        if (markerPrefab != null)
        {
            marker = Instantiate(markerPrefab);
            marker.SetActive(false);
        }
    }

    private void OnEnable()
    {
        pointAction.action.Enable();
        selectAction.action.Enable();
        cancelAction.action.Enable();
        swapAction.action.Enable();

        selectAction.action.performed += OnSelect;
        cancelAction.action.performed += OnCancel;
        swapAction.action.performed += OnSwap;
        GameEvents.PlayerDied += OnPlayerDied;
    }

    private void OnDisable()
    {
        selectAction.action.performed -= OnSelect;
        cancelAction.action.performed -= OnCancel;
        swapAction.action.performed -= OnSwap;
        GameEvents.PlayerDied -= OnPlayerDied;

        // domain reload is off so actions stay on between plays unless turned off here
        pointAction.action.Disable();
        selectAction.action.Disable();
        cancelAction.action.Disable();
        swapAction.action.Disable();
    }

    private void OnSelect(InputAction.CallbackContext context) => TrySelect();
    private void OnCancel(InputAction.CallbackContext context) => CancelSelection();
    private void OnSwap(InputAction.CallbackContext context) => TrySwap();

    // drop the target on death so it can't carry over into the reset room
    private void OnPlayerDied(string cause) => CancelSelection();

    // keep the marker over the target, drop the selection if the target got destroyed or switched off (killed)
    private void LateUpdate()
    {
        if (!hasTarget) return;

        if (target == null || !target.gameObject.activeInHierarchy)
        {
            CancelSelection();
            return;
        }

        if (marker != null) marker.transform.position = target.position + Vector3.up * markerHeight;
    }

    private void TrySelect()
    {
        if (InNoSelectZone())
        {
            Debug.Log("[Swapper] In a no select zone, selection blocked.");
            GameEvents.RaiseSelectionBlocked();
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(pointAction.action.ReadValue<Vector2>());
        // clicking empty floor isn't a failed selection, so it stays silent
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, swappableLayers, QueryTriggerInteraction.Ignore)) return;

        if (!IsInRange(hit.transform))
        {
            GameEvents.RaiseSelectionBlocked();
            return;
        }

        if (!HasLineOfSight(hit.collider))
        {
            Debug.Log("[Swapper] No line of sight to target, selection blocked.");
            GameEvents.RaiseSelectionBlocked();
            return;
        }

        SetSelection(hit.transform);
    }

    // zones are triggers so the overlap has to include them
    private bool InNoSelectZone()
    {
        foreach (Collider hit in Physics.OverlapSphere(transform.position, 0.1f, ~0, QueryTriggerInteraction.Collide))
            if (hit.GetComponent<NoSelectZone>() != null) return true;
        return false;
    }

    private void CancelSelection() => SetSelection(null);

    private void SetSelection(Transform newTarget)
    {
        bool selecting = newTarget != null;
        if (!selecting && !hasTarget) return;
        if (selecting && hasTarget && newTarget == target) return;

        target = newTarget;
        hasTarget = selecting;
        if (marker != null) marker.SetActive(selecting);
        GameEvents.RaiseSelectionChanged(newTarget);
    }

    private bool IsInRange(Transform other) => Vector3.Distance(transform.position, other.position) <= swapRange;

    private bool HasLineOfSight(Collider targetCollider)
    {
        Vector3 eye = transform.position + Vector3.up * eyeHeight;

        // a swappable in the way blocks anything behind it, the target itself still counts as seen below
        LayerMask blockers = swappablesBlockSight ? sightBlockers | swappableLayers : sightBlockers;

        if (!Physics.Linecast(eye, targetCollider.bounds.center, out RaycastHit hit, blockers, QueryTriggerInteraction.Ignore))
            return true;

        return hit.collider == targetCollider;
    }

    private void TrySwap()
    {
        if (target == null) return;
        if (Time.unscaledTime < nextSwapTime) return;

        Vector3 byteFeet = GetFeet(transform);
        Vector3 targetFeet = GetFeet(target);
        // checked before anything moves, using the target's own width so a wide enemy on a pit rim counts as on the floor
        bool floorUnderByte = HasGroundBelow(byteFeet, FootprintRadius(target), target);

        MoveTo(transform, transform.position + (targetFeet - byteFeet), targetFeet, true);
        MoveTo(target, target.position + (byteFeet - targetFeet), byteFeet, floorUnderByte);
        Physics.SyncTransforms();
        nextSwapTime = Time.unscaledTime + cooldown;

        if (target.TryGetComponent(out ITeleportable swapped)) swapped.OnTeleported();
        GameEvents.RaisePlayerSwapped(target);
        CancelSelection();
    }

    private void MoveTo(Transform obj, Vector3 position, Vector3 feet, bool onFloor)
    {
        if (obj.TryGetComponent(out CharacterController controller))
        {
            // controller has to be off or it snaps back
            controller.enabled = false;
            obj.position = position;
            controller.enabled = true;
            return;
        }

        if (obj.TryGetComponent(out NavMeshAgent agent))
        {
            WarpAgent(agent, position, feet, onFloor);
            return;
        }

        obj.position = position;
    }

    private void WarpAgent(NavMeshAgent agent, Vector3 position, Vector3 feet, bool onFloor)
    {
        // only snap when there's real floor underneath, an enemy over a pit always falls
        if (onFloor
            && NavMesh.SamplePosition(feet, out NavMeshHit hit, EdgeSnapDistance, NavMesh.AllAreas)
            && Mathf.Abs(hit.position.y - feet.y) <= SameFloorHeight)
        {
            // warp to body height, warping to the navmesh point leaves the enemy half in the floor
            agent.Warp(hit.position + Vector3.up * agent.baseOffset);
            return;
        }

        // no floor, or no navmesh on this floor (trap door shaft), turn the agent off and let the enemy fall
        agent.enabled = false;
        agent.transform.position = position;
        if (agent.TryGetComponent(out Rigidbody body)) body.isKinematic = false;
    }

    // true if any floor is under a circle this wide round the feet, ignoring byte and the swap target
    // a single ray from the middle missed the rim when byte stood with his centre just past a pit edge
    private bool HasGroundBelow(Vector3 feet, float radius, Transform ignore)
    {
        Vector3 start = feet + Vector3.up * (GroundCheckUp + radius);
        foreach (RaycastHit hit in Physics.SphereCastAll(start, radius, Vector3.down, GroundCheckUp + GroundCheckDown, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform.IsChildOf(transform) || hit.transform.IsChildOf(ignore)) continue;
            // started inside something, like a wall beside byte, so it isn't floor under him
            if (hit.distance <= 0f) continue;
            if (hit.normal.y < MinFloorNormal) continue;
            return true;
        }
        return false;
    }

    // half the target's narrowest width on the ground
    private static float FootprintRadius(Transform obj)
    {
        Collider col = obj.GetComponent<Collider>();
        if (col == null) return 0.1f;
        Vector3 extents = col.bounds.extents;
        return Mathf.Max(0.05f, Mathf.Min(extents.x, extents.z));
    }

    // bottom centre of an object's collider, used as its feet for swapping
    private static Vector3 GetFeet(Transform obj)
    {
        Collider col = obj.TryGetComponent(out CharacterController cc) ? cc : obj.GetComponent<Collider>();
        if (col == null) return obj.position;
        return new Vector3(obj.position.x, col.bounds.min.y, obj.position.z);
    }
}
