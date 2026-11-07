using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour, ITeleportable, IResettable
{
    [Header("Detection")]
    [SerializeField] private float _detectionRange = 10f;
    [SerializeField] private float _attackRange = 2f;

    private EnemyPerception _perception;

    [Header("Investigate")]
    [SerializeField] private float _investigateDuration = 5f;

    [Header("Patrol")]
    [SerializeField] private float _wanderRadius = 8f;
    [SerializeField] private float _wanderWaitMin = 1f;
    [SerializeField] private float _wanderWaitMax = 3f;
    [SerializeField] private Transform[] _patrolPoints;

    [Header("Confused (Post-Teleport)")]
    [SerializeField] private float _confusedDuration = 3f;
    [SerializeField] private float _confusedTurnSpeed = 120f;

    [Header("Destabilise")]
    // turn off for enemies that shouldn't die from a second swap
    [SerializeField] private bool _secondSwapKills = true;
    [SerializeField] private float _killGraceTime = 0.25f;

    private float _confusedEndedAt = -999f;

    [Header("Attack")]
    [SerializeField] private float _attackDamage = 10f;
    [SerializeField] private float _attackCooldown = 1.5f;

    

    private float _attackTimer = 0f;

    private int _currentPatrolIndex = 0;
    private bool _isWaiting = false;
    private float _confusedTimer = 0f;
    private float _wanderWaitTimer = 0f;
    private float _wanderWaitDuration = 0f;

    public enum AIState { Patrol, Chase, Investigate, Confused }
    private AIState _currentState = AIState.Patrol;
    public AIState CurrentState => _currentState;

    private NavMeshAgent _agent;
    private Transform _player;
    private Vector3? _lastKnownPosition;
    private float _investigateTimer;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _perception = GetComponent<EnemyPerception>();
        _agent.stoppingDistance = _attackRange;
    }

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;
        else
            Debug.LogError("[EnemyAI] No GameObject tagged 'Player' found!");

        StartPatrolling();
    }

    void StartPatrolling()
    {
        _currentPatrolIndex = 0;
        if (_patrolPoints != null && _patrolPoints.Length > 0)
            _agent.SetDestination(_patrolPoints[0].position);
        else
            SetNewWanderDestination();
    }

    void Update()
    {
        if (_player == null) return;
        // agent gets turned off while falling into a pit
        if (!_agent.enabled) return;

        switch (_currentState)
        {
            case AIState.Patrol:      UpdatePatrol();      break;
            case AIState.Chase:       UpdateChase();       break;
            case AIState.Investigate: UpdateInvestigate(); break;
            case AIState.Confused:    UpdateConfused();    break;
        }
    }

    void UpdatePatrol()
    {
        if (CanSeePlayer())
        {
            _lastKnownPosition = _player.position;
            _isWaiting = false;
            TransitionTo(AIState.Chase);
            return;
        }

        if (_patrolPoints != null && _patrolPoints.Length > 0)
            UpdatePatrolPoints();
        else
            UpdateRandomWander();
    }

    void UpdatePatrolPoints()
    {
        if (_agent.remainingDistance <= _agent.stoppingDistance && !_isWaiting)
        {
            _isWaiting = true;
            _wanderWaitDuration = Random.Range(_wanderWaitMin, _wanderWaitMax);
            _wanderWaitTimer = 0f;
        }

        if (_isWaiting)
        {
            _wanderWaitTimer += Time.deltaTime;
            if (_wanderWaitTimer >= _wanderWaitDuration)
            {
                _isWaiting = false;
                _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Length;
                _agent.SetDestination(_patrolPoints[_currentPatrolIndex].position);
            }
        }
    }

    void UpdateRandomWander()
    {
        if (_agent.remainingDistance <= _agent.stoppingDistance && !_isWaiting)
        {
            _isWaiting = true;
            _wanderWaitDuration = Random.Range(_wanderWaitMin, _wanderWaitMax);
            _wanderWaitTimer = 0f;
        }

        if (_isWaiting)
        {
            _wanderWaitTimer += Time.deltaTime;
            if (_wanderWaitTimer >= _wanderWaitDuration)
            {
                _isWaiting = false;
                SetNewWanderDestination();
            }
        }
    }

    void SetNewWanderDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * _wanderRadius;
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, _wanderRadius, NavMesh.AllAreas))
            _agent.SetDestination(hit.position);
    }

    void UpdateChase()
{
    if (!CanSeePlayer(true))
    {
        _agent.stoppingDistance = 0f;
        TransitionTo(AIState.Investigate);
        return;
    }

    _lastKnownPosition = _player.position;
    float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

    if (distanceToPlayer <= _attackRange)
    {
        // Stop and attack
        _agent.stoppingDistance = _attackRange;
        _agent.SetDestination(_player.position);

        _attackTimer += Time.deltaTime;
        if (_attackTimer >= _attackCooldown)
        {
            _attackTimer = 0f;
            // one hit kills for now, damage not used
            if (_player.TryGetComponent(out IKillable killable))
                killable.Kill(gameObject.name);
        }
    }
    else
    {
        // Player moved out of attack range — chase them
        _attackTimer = 0f;
        _agent.stoppingDistance = 0f;
        _agent.SetDestination(_player.position);
    }
}

    void UpdateInvestigate()
    {
        if (!_lastKnownPosition.HasValue)
        {
            Debug.Log("[EnemyAI] No last known position (teleported). Returning to Patrol.");
            TransitionTo(AIState.Patrol);
            return;
        }

        _agent.SetDestination(_lastKnownPosition.Value);

        if (CanSeePlayer(true))
        {
            _lastKnownPosition = _player.position;
            TransitionTo(AIState.Chase);
            return;
        }

        _investigateTimer += Time.deltaTime;
        if (_investigateTimer >= _investigateDuration)
        {
            _investigateTimer = 0f;
            TransitionTo(AIState.Patrol);
        }
    }

    void UpdateConfused()
    {
        _agent.ResetPath();
        transform.Rotate(0f, _confusedTurnSpeed * Time.deltaTime, 0f);

        // stays stunned for the full time even if it can see byte, otherwise the second swap is impossible
        _confusedTimer += Time.deltaTime;
        if (_confusedTimer >= _confusedDuration)
        {
            _confusedTimer = 0f;
            _confusedEndedAt = Time.time;
            TransitionTo(AIState.Patrol);
            StartPatrolling();
        }
    }

    bool IsStillStunned() => _currentState == AIState.Confused || Time.time - _confusedEndedAt <= _killGraceTime;

    bool CanSeePlayer(bool alerted = false) => _perception.CanSeePlayer(transform, _player, alerted);

    public void OnTeleported()
    {
        // swapped again while stunned (or just after) = destabilised, so it dies
        if (_secondSwapKills && IsStillStunned())
        {
            if (TryGetComponent(out IKillable killable)) killable.Kill("destabilise");
            return;
        }

        _confusedTimer = 0f;
        _lastKnownPosition = null;
        _investigateTimer = 0f;
        _isWaiting = false;
        TransitionTo(AIState.Confused);
        Debug.Log($"[EnemyAI] {gameObject.name} teleported — entering Confused state.");
    }

    public void ResetState()
    {
        _lastKnownPosition = null;
        _investigateTimer = 0f;
        _confusedTimer = 0f;
        _confusedEndedAt = -999f;
        _attackTimer = 0f;
        _isWaiting = false;
        _agent.stoppingDistance = _attackRange;
        _agent.ResetPath();
        TransitionTo(AIState.Patrol);
        StartPatrolling();
    }

    void TransitionTo(AIState newState)
    {
        if (_currentState == newState) return;
        Debug.Log($"[EnemyAI] {gameObject.name}: {_currentState} → {newState}");
        _currentState = newState;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _wanderRadius);
    }

    




}