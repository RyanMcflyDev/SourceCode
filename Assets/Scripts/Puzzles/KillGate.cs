using UnityEngine;

// a gate that only opens once its guard is dead, so the only way through is the double swap
// comes back when byte dies, the room reset brings the guard back too
public class KillGate : MonoBehaviour
{
    [SerializeField] private EnemyDeath guard;
    // switched off to open, holds the blocking collider and the visuals
    [SerializeField] private GameObject gate;
    // lets the guard's dissolve play before the way opens
    [SerializeField] private float openDelay = 0.6f;

    private float deadTime = -1f;

    private void OnEnable() => GameEvents.PlayerDied += OnPlayerDied;
    private void OnDisable() => GameEvents.PlayerDied -= OnPlayerDied;

    private void OnPlayerDied(string cause)
    {
        deadTime = -1f;
        gate.SetActive(true);
    }

    private void Update()
    {
        if (!gate.activeSelf || guard == null) return;

        if (!guard.IsDead)
        {
            deadTime = -1f;
            return;
        }

        if (deadTime < 0f) deadTime = Time.time;
        if (Time.time - deadTime >= openDelay) gate.SetActive(false);
    }
}
