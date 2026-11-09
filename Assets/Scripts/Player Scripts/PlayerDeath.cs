using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerDeath : MonoBehaviour, IKillable
{
    [Header("Respawn")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float respawnDelay = 0.25f;

    private CharacterController controller;

    public bool IsDead { get; private set; }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (spawnPoint == null)
            Debug.LogWarning("[PlayerDeath] No spawn point set");
    }

    private void OnEnable() => GameEvents.SpawnPointChanged += OnSpawnPointChanged;
    private void OnDisable() => GameEvents.SpawnPointChanged -= OnSpawnPointChanged;

    private void OnSpawnPointChanged(Transform newSpawnPoint) => spawnPoint = newSpawnPoint;

    public void Kill(string cause)
    {
        // byte has two colliders so this can get called twice
        if (IsDead) return;
        IsDead = true;

        Debug.Log($"[PlayerDeath] Killed by {cause}");
        GameEvents.RaisePlayerDied(cause);

        Invoke(nameof(Respawn), respawnDelay);
    }

    private void Respawn()
    {
        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;
        float feetOffset = transform.position.y - controller.bounds.min.y;

        // same as swapper, controller has to be off or it snaps back
        controller.enabled = false;
        transform.position = spawnPosition + Vector3.up * feetOffset;
        controller.enabled = true;

        foreach (var resettable in GetComponents<IResettable>())
            resettable.ResetState();

        IsDead = false;
        GameEvents.RaisePlayerRespawned();
    }
}
