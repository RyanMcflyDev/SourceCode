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

    private const float NavMeshSnapDistance = 2f;
    // how far sideways an enemy can be pulled onto the navmesh, keep small so they fall into pits
    private const float LedgeSnapDistance = 0.5f;

    private Transform target;
    private GameObject marker;
    private float nextSwapTime;

    private void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        marker = Instantiate(markerPrefab);
        marker.SetActive(false);
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

    // keep the marker over the target, cancel if the target got destroyed
    private void LateUpdate()
    {
        if (!marker.activeSelf) return;

        if (target == null)
        {
            CancelSelection();
            return;
        }

        marker.transform.position = target.position + Vector3.up * markerHeight;
    }

    private void TrySelect()
    {
        if (InNoSelectZone())
        {
            Debug.Log("[Swapper] In a no select zone, selection blocked.");
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(pointAction.action.ReadValue<Vector2>());
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, swappableLayers, QueryTriggerInteraction.Ignore)) return;
        if (!IsInRange(hit.transform)) return;

        if (!HasLineOfSight(hit.collider))
        {
            Debug.Log("[Swapper] No line of sight to target, selection blocked.");
            return;
        }

        target = hit.transform;
        marker.SetActive(true);
    }

    // zones are triggers so the overlap has to include them
    private bool InNoSelectZone()
    {
        foreach (Collider hit in Physics.OverlapSphere(transform.position, 0.1f, ~0, QueryTriggerInteraction.Collide))
            if (hit.GetComponent<NoSelectZone>() != null) return true;
        return false;
    }

    private void CancelSelection()
    {
        target = null;
        marker.SetActive(false);
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

        MoveTo(transform, transform.position + (targetFeet - byteFeet));
        MoveTo(target, target.position + (byteFeet - targetFeet));
        Physics.SyncTransforms();
        nextSwapTime = Time.unscaledTime + cooldown;

        if (target.TryGetComponent(out ITeleportable swapped)) swapped.OnTeleported();
        GameEvents.RaisePlayerSwapped(target);
        CancelSelection();
    }

    private void MoveTo(Transform obj, Vector3 position)
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
            WarpAgent(agent, position);
            return;
        }

        obj.position = position;
    }

    private void WarpAgent(NavMeshAgent agent, Vector3 position)
    {
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, NavMeshSnapDistance, NavMesh.AllAreas))
        {
            // only snap if the navmesh is basically underneath, otherwise it was the pit edge
            Vector3 sideways = hit.position - position;
            sideways.y = 0f;

            if (sideways.magnitude <= LedgeSnapDistance)
            {
                // warp to body height, warping to the navmesh point leaves the enemy half in the floor
                agent.Warp(hit.position + Vector3.up * agent.baseOffset);
                return;
            }
        }

        // no navmesh underneath, turn the agent off and let the enemy fall
        agent.enabled = false;
        agent.transform.position = position;
        if (agent.TryGetComponent(out Rigidbody body)) body.isKinematic = false;
    }

    // bottom centre of an object's collider, used as its feet for swapping
    private static Vector3 GetFeet(Transform obj)
    {
        Collider col = obj.TryGetComponent(out CharacterController cc) ? cc : obj.GetComponent<Collider>();
        if (col == null) return obj.position;
        return new Vector3(obj.position.x, col.bounds.min.y, obj.position.z);
    }
}
