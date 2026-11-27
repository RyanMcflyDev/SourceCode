using UnityEngine;
using UnityEngine.Events;

// shuts the door behind byte once he and the guard are both in the room, so the guard can't be swapped out of reach
// only locks if the guard is inside too, so swapping him out first never traps byte with no way to finish
[RequireComponent(typeof(Collider))]
public class LockInWithGuard : MonoBehaviour
{
    [SerializeField] private Door door;
    [SerializeField] private EnemyDeath guard;
    // hook glitch's line up here
    [SerializeField] private UnityEvent onLocked;

    private Collider room;
    private Transform player;
    private bool locked;

    private void Reset() => GetComponent<Collider>().isTrigger = true;

    private void Awake() => room = GetComponent<Collider>();

    private void Start()
    {
        GameObject found = GameObject.FindWithTag("Player");
        if (found != null) player = found.transform;
    }

    private void OnEnable() => GameEvents.PlayerRespawned += OnPlayerRespawned;
    private void OnDisable() => GameEvents.PlayerRespawned -= OnPlayerRespawned;

    // on respawn not on death, byte lies in the room with the guard until he respawns and would lock it again
    private void OnPlayerRespawned()
    {
        locked = false;
        door.Open();
    }

    private void Update()
    {
        if (player == null || guard == null) return;

        // guard's gone, nothing to keep byte in for
        if (locked && guard.IsDead)
        {
            locked = false;
            door.Open();
            return;
        }

        if (locked || guard.IsDead) return;

        if (room.bounds.Contains(player.position) && room.bounds.Contains(guard.transform.position))
        {
            locked = true;
            door.Close();
            onLocked.Invoke();
        }
    }
}
