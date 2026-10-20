using UnityEngine;

// put this on a trigger in the room doorway, byte respawns here after this
[RequireComponent(typeof(Collider))]
public class RoomEntrance : MonoBehaviour
{
    [SerializeField] private Transform _spawnPoint;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_spawnPoint == null) return;

        GameEvents.RaiseSpawnPointChanged(_spawnPoint);
    }
}
