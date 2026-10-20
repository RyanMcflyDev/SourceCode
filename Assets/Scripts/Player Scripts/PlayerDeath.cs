using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerDeath : MonoBehaviour, IKillable
{
    [Header("Respawn")]
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private float _respawnDelay = 0.25f;

    private CharacterController _controller;
    private bool _isDead;

    public bool IsDead => _isDead;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();

        if (_spawnPoint == null)
            Debug.LogWarning("[PlayerDeath] No spawn point set");
    }

    private void OnEnable() => GameEvents.SpawnPointChanged += OnSpawnPointChanged;
    private void OnDisable() => GameEvents.SpawnPointChanged -= OnSpawnPointChanged;

    private void OnSpawnPointChanged(Transform spawnPoint) => _spawnPoint = spawnPoint;

    public void Kill(string cause)
    {
        // byte has two colliders so this can get called twice
        if (_isDead) return;
        _isDead = true;

        Debug.Log($"[PlayerDeath] Killed by {cause}");
        GameEvents.RaisePlayerDied(cause);

        Invoke(nameof(Respawn), _respawnDelay);
    }

    private void Respawn()
    {
        Vector3 spawnPosition = _spawnPoint != null ? _spawnPoint.position : transform.position;
        float feetOffset = transform.position.y - _controller.bounds.min.y;

        // same as swapper, controller has to be off or it snaps back
        _controller.enabled = false;
        transform.position = spawnPosition + Vector3.up * feetOffset;
        _controller.enabled = true;

        foreach (var resettable in GetComponents<IResettable>())
            resettable.ResetState();

        _isDead = false;
        GameEvents.RaisePlayerRespawned();
    }
}
