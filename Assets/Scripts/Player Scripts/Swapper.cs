using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;


public class Swapper : MonoBehaviour
{
  
  [Header("input")]
  [SerializeField] private InputActionReference _swapAction;
  [SerializeField] private InputActionReference _pointAction;
  [SerializeField] private InputActionReference _selectAction;
  [SerializeField] private InputActionReference _cancelAction;


  [Header("References")]
  [SerializeField] private Camera _camera;
  [SerializeField] private GameObject _markerPrefab;


  [Header("Targeting")]
    [SerializeField] private LayerMask _swappableLayers;
    // only these block line of sight, enemies shouldn't
    [SerializeField] private LayerMask _sightBlockers;
    [Tooltip("Swappable objects block sight to things behind them, but can still be swapped with themselves")]
    [SerializeField] private bool _swappablesBlockSight = true;
    [SerializeField] private float _swapRange = 7f;
    [SerializeField] private float _eyeHeight = 1.2f;
    [SerializeField] private float _markerHeight = 1.5f;


  [Header("Timing")]
    [SerializeField] private float _cooldown = 1f;



   private const float NavMeshSnapDistance = 2f;
   // how far sideways an enemy can be pulled onto the navmesh, keep small so they fall into pits
   private const float LedgeSnapDistance = 0.5f;

   private Transform _target;
   private GameObject _marker;
   private float _nextSwapTime;


   private void Awake()
   {
       if (_camera == null)
           _camera = Camera.main;
       
       _marker = Instantiate(_markerPrefab);
       _marker.SetActive(false);

   }

// Input handling
   private void OnEnable()
   {
    _pointAction.action.Enable();
    _selectAction.action.Enable();
    _cancelAction.action.Enable();
    _swapAction.action.Enable();
    
    _selectAction.action.performed += OnSelect;
    _cancelAction.action.performed += OnCancel;
    _swapAction.action.performed += OnSwap;

    GameEvents.PlayerDied += OnPlayerDied;
    
    
    }
    
    private void OnDisable()
    {
        
        _selectAction.action.performed -= OnSelect;
        _cancelAction.action.performed -= OnCancel;
        _swapAction.action.performed -= OnSwap;
        GameEvents.PlayerDied -= OnPlayerDied;

        // domain reload is off so actions stay on between plays unless turned off here
        _pointAction.action.Disable();
        _selectAction.action.Disable();
        _cancelAction.action.Disable();
        _swapAction.action.Disable();
    }
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

// Input callbacks
    private void OnSelect(InputAction.CallbackContext context) => TrySelect();
    private void OnCancel(InputAction.CallbackContext context) => CancelSelection();
    private void OnSwap(InputAction.CallbackContext context) => TrySwap();

    // drop the target on death so it can't carry over into the reset room
    private void OnPlayerDied(string cause) => CancelSelection();

// Update the marker position and check if the target is still valid after frame is called
    private void LateUpdate()
    {
        if (!(_marker.activeSelf)) return;

        // Target was destroyed while selected, cancel selection
        if (_target == null)
        {
            CancelSelection();
            return;
        }

        // Update marker position
        _marker.transform.position = _target.position + Vector3.up * _markerHeight;
    
    }
// Attempt to select a target
    private void TrySelect()
    {
        Ray ray = _camera.ScreenPointToRay(_pointAction.action.ReadValue<Vector2>());
        if (!Physics.Raycast(ray, out RaycastHit hit , Mathf.Infinity, _swappableLayers, QueryTriggerInteraction.Ignore)) return;

        if (!IsInRange(hit.transform)) return; 

        if (!HasLineOfSight(hit.collider))
        {
            Debug.Log("[Swapper] No line of sight to target, selection blocked.");
            return;
        }


        _target = hit.transform;
        _marker.SetActive(true);
    }


//cancel target selection
    private void CancelSelection()
    {
        _target = null;
        _marker.SetActive(false);
    }
// Attempt to swap with the selected target
    private bool IsInRange(Transform target)
    {
        return Vector3.Distance(transform.position, target.position) <= _swapRange;
    
    }
// Check if the target is within line of sight
    private bool HasLineOfSight(Collider targetCollider)
    {
       Vector3 eye = transform.position + Vector3.up * _eyeHeight;

       // a swappable in the way blocks anything behind it, the target itself still counts as seen below
       LayerMask blockers = _swappablesBlockSight ? _sightBlockers | _swappableLayers : _sightBlockers;

       // Check if the target is within line of sight
       if (!Physics.Linecast(eye, targetCollider.bounds.center, out RaycastHit hit, blockers, QueryTriggerInteraction.Ignore))
       return true;

       return hit.collider == targetCollider;
      
    }

// Swap the player and the target positions
    private void TrySwap()
    {
        if (_target == null) return;
        if (Time.unscaledTime < _nextSwapTime) return;
        

        Vector3 byteFeet = GetFeet(transform);
        Vector3 targetFeet = GetFeet(_target);

        MoveTo(transform, transform.position + (targetFeet - byteFeet));
        MoveTo(_target, _target.position + (byteFeet - targetFeet));
        Physics.SyncTransforms();
        _nextSwapTime = Time.unscaledTime + _cooldown;

        if (_target.TryGetComponent(out ITeleportable swapped)) swapped.OnTeleported();
        GameEvents.RaisePlayerSwapped(_target);
        CancelSelection();
    }


    private void MoveTo(Transform obj, Vector3 position)
    {
        if (obj.TryGetComponent(out CharacterController controller))
        {
            // Disable the CharacterController to avoid collision issues during teleportation
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
                agent.Warp(hit.position);
                return;
            }
        }

        // If no valid NavMesh position is found, let the enemy fall to the ground by disabling the NavMeshAgent and setting the position directly
        agent.enabled = false;
        agent.transform.position = position;
        if (agent.TryGetComponent(out Rigidbody body)) body.isKinematic = false;

    }


    // Bottom-centre of an object's collider, used as its "feet" for swapping.
    private static Vector3 GetFeet(Transform obj)
    {
        Collider col = obj.TryGetComponent(out CharacterController cc) ? cc : obj.GetComponent<Collider>();
        if (col == null) return obj.position;
        return new Vector3(obj.position.x, col.bounds.min.y, obj.position.z);
    }
}
