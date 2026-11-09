using UnityEngine;
using UnityEngine.AI;

// brings dead enemies back where they started, plates call RespawnDead through their events
public class EnemyRespawner : MonoBehaviour
{
    [SerializeField] private EnemyDeath[] enemies;
    // waits until byte moves away so an enemy never spawns on top of him
    [SerializeField] private float safeDistance = 2.5f;

    private Vector3[] startPositions;
    private Quaternion[] startRotations;
    private bool[] waiting;
    private Transform player;

    private void Awake()
    {
        startPositions = new Vector3[enemies.Length];
        startRotations = new Quaternion[enemies.Length];
        waiting = new bool[enemies.Length];

        for (int i = 0; i < enemies.Length; i++)
        {
            startPositions[i] = enemies[i].transform.position;
            startRotations[i] = enemies[i].transform.rotation;
        }

        player = GameObject.FindWithTag("Player").transform;
    }

    private void OnEnable() => GameEvents.PlayerDied += OnPlayerDied;
    private void OnDisable() => GameEvents.PlayerDied -= OnPlayerDied;

    // the room reset already brings everyone back
    private void OnPlayerDied(string cause) => System.Array.Clear(waiting, 0, waiting.Length);

    public void RespawnDead()
    {
        for (int i = 0; i < enemies.Length; i++)
            if (enemies[i].IsDead) waiting[i] = true;
    }

    private void Update()
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            if (!waiting[i]) continue;
            if (Vector3.Distance(player.position, startPositions[i]) < safeDistance) continue;

            waiting[i] = false;
            Respawn(i);
        }
    }

    private void Respawn(int i)
    {
        GameObject enemy = enemies[i].gameObject;
        enemy.SetActive(true);

        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
        agent.enabled = true;
        agent.Warp(startPositions[i]);
        enemy.transform.rotation = startRotations[i];

        foreach (IResettable resettable in enemy.GetComponents<IResettable>())
            resettable.ResetState();
    }
}
