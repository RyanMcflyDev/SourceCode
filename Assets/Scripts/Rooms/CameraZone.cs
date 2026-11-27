using UnityEngine;

// switches this area's camera on while byte is inside, give it a higher priority than the level's main camera
[RequireComponent(typeof(Collider))]
public class CameraZone : MonoBehaviour
{
    [SerializeField] private GameObject areaCamera;

    private Collider zone;

    private void Reset() => GetComponent<Collider>().isTrigger = true;

    private void Awake() => zone = GetComponent<Collider>();

    private void OnEnable() => GameEvents.PlayerRespawned += OnPlayerRespawned;
    private void OnDisable() => GameEvents.PlayerRespawned -= OnPlayerRespawned;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) areaCamera.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) areaCamera.SetActive(false);
    }

    // respawn turns byte's controller off to move him, which never fires OnTriggerExit,
    // so check where he actually ended up instead
    private void OnPlayerRespawned()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;
        areaCamera.SetActive(zone.bounds.Contains(player.transform.position));
    }
}
