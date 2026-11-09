using UnityEngine;

// put this on a trigger in the room doorway, byte respawns here after this
[RequireComponent(typeof(Collider))]
public class RoomEntrance : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;

    private void Reset() => GetComponent<Collider>().isTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (spawnPoint == null) return;

        GameEvents.RaiseSpawnPointChanged(spawnPoint);
    }
}
