using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour, ITeleportable, IResettable
{
    [Header("Attack")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Investigate")]
    [SerializeField] private float investigateDuration = 5f;

    [Header("Patrol")]
    [SerializeField] private float wanderRadius = 8f;
    [SerializeField] private float wanderWaitMin = 1f;
    [SerializeField] private float wanderWaitMax = 3f;
    [SerializeField] private Transform[] patrolPoints;

    [Header("Confused (after a swap)")]
    [SerializeField] private float confusedDuration = 3f;
    [SerializeField] private float confusedTurnSpeed = 120f;

    [Header("Destabilise")]
    // turn off for enemies that shouldn't die from a second swap
    [SerializeField] private bool secondSwapKills = true;
    [SerializeField] private float killGraceTime = 0.25f;

    public enum AIState { Patrol, Chase, Investigate, Confused }
    public AIState CurrentState { get; private set; } = AIState.Patrol;

    private NavMeshAgent agent;
    private EnemyPerception perception;
    private Transform player;
    private Vector3? lastKnownPosition;

    private float attackTimer;
    private float investigateTimer;
    private float confusedTimer;
    private float confusedEndedAt = -999f;
    private int currentPatrolIndex;
    private bool isWaiting;
    private float waitTimer;
    private float waitDuration;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        perception = GetComponent<EnemyPerception>();
        agent.stoppingDistance = attackRange;
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
            player = playerObject.transform;
        else
            Debug.LogError("[EnemyAI] No GameObject tagged 'Player' found!");

        StartPatrolling();
    }

    private void Update()
    {
        if (player == null) return;
        // agent gets turned off while falling into a pit
        if (!agent.enabled) return;

        switch (CurrentState)
        {
            case AIState.Patrol:      UpdatePatrol();      break;
            case AIState.Chase:       UpdateChase();       break;
            case AIState.Investigate: UpdateInvestigate(); break;
            case AIState.Confused:    UpdateConfused();    break;
        }
    }

    private void StartPatrolling()
    {
        currentPatrolIndex = 0;
        if (patrolPoints != null && patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[0].position);
        else
            SetNewWanderDestination();
    }

    private void UpdatePatrol()
    {
        if (CanSeePlayer())
        {
            lastKnownPosition = player.position;
            isWaiting = false;
            TransitionTo(AIState.Chase);
            return;
        }

        bool hasPatrol = patrolPoints != null && patrolPoints.Length > 0;
        if (!WaitAtDestination()) return;

        if (hasPatrol)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
        else
        {
            SetNewWanderDestination();
        }
    }

    // true once the enemy has reached its destination and waited there, ready to pick the next one
    private bool WaitAtDestination()
    {
        if (agent.remainingDistance <= agent.stoppingDistance && !isWaiting)
        {
            isWaiting = true;
            waitDuration = Random.Range(wanderWaitMin, wanderWaitMax);
            waitTimer = 0f;
        }

        if (!isWaiting) return false;

        waitTimer += Time.deltaTime;
        if (waitTimer < waitDuration) return false;

        isWaiting = false;
        return true;
    }

    private void SetNewWanderDestination()
    {
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * wanderRadius;
        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    private void UpdateChase()
    {
        if (!CanSeePlayer(true))
        {
            agent.stoppingDistance = 0f;
            TransitionTo(AIState.Investigate);
            return;
        }

        lastKnownPosition = player.position;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            // stop and attack
            agent.stoppingDistance = attackRange;
            agent.SetDestination(player.position);

            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                attackTimer = 0f;
                // one hit kills for now
                if (player.TryGetComponent(out IKillable killable))
                    killable.Kill(gameObject.name);
            }
        }
        else
        {
            // byte moved out of attack range, chase him
            attackTimer = 0f;
            agent.stoppingDistance = 0f;
            agent.SetDestination(player.position);
        }
    }

    private void UpdateInvestigate()
    {
        // no last known position after a teleport, go back to patrolling
        if (!lastKnownPosition.HasValue)
        {
            TransitionTo(AIState.Patrol);
            return;
        }

        agent.SetDestination(lastKnownPosition.Value);

        if (CanSeePlayer(true))
        {
            lastKnownPosition = player.position;
            TransitionTo(AIState.Chase);
            return;
        }

        investigateTimer += Time.deltaTime;
        if (investigateTimer >= investigateDuration)
        {
            investigateTimer = 0f;
            TransitionTo(AIState.Patrol);
        }
    }

    private void UpdateConfused()
    {
        agent.ResetPath();
        transform.Rotate(0f, confusedTurnSpeed * Time.deltaTime, 0f);

        // stays stunned for the full time even if it can see byte, otherwise the second swap is impossible
        confusedTimer += Time.deltaTime;
        if (confusedTimer >= confusedDuration)
        {
            confusedTimer = 0f;
            confusedEndedAt = Time.time;
            TransitionTo(AIState.Patrol);
            StartPatrolling();
        }
    }

    private bool IsStillStunned() => CurrentState == AIState.Confused || Time.time - confusedEndedAt <= killGraceTime;

    private bool CanSeePlayer(bool alerted = false) => perception.CanSeePlayer(transform, player, alerted);

    public void OnTeleported()
    {
        // swapped again while stunned (or just after) = destabilised, so it dies
        if (secondSwapKills && IsStillStunned())
        {
            if (TryGetComponent(out IKillable killable)) killable.Kill("destabilise");
            return;
        }

        confusedTimer = 0f;
        lastKnownPosition = null;
        investigateTimer = 0f;
        isWaiting = false;
        TransitionTo(AIState.Confused);
    }

    public void ResetState()
    {
        lastKnownPosition = null;
        investigateTimer = 0f;
        confusedTimer = 0f;
        confusedEndedAt = -999f;
        attackTimer = 0f;
        isWaiting = false;
        agent.stoppingDistance = attackRange;
        agent.ResetPath();
        TransitionTo(AIState.Patrol);
        StartPatrolling();
    }

    private void TransitionTo(AIState newState)
    {
        if (CurrentState == newState) return;
        // LogStateChange(newState);
        CurrentState = newState;
    }

    // uncomment this method and the call in TransitionTo to log every state change
    // private void LogStateChange(AIState newState)
    // {
    //     Debug.Log($"[EnemyAI] {gameObject.name}: {CurrentState} to {newState}");
    // }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
    }
}
