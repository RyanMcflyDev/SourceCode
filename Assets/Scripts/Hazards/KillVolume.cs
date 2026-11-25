using UnityEngine;

[RequireComponent(typeof(Collider))]
public class KillVolume : MonoBehaviour
{
    [SerializeField] private string cause = "pit";
    // corrupted floor enemies can walk on but byte can't
    [SerializeField] private bool playerOnly = false;

    private void Reset() => GetComponent<Collider>().isTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        if (playerOnly && !other.CompareTag("Player")) return;
        var killable = other.GetComponentInParent<IKillable>();
        if (killable != null) killable.Kill(cause);
    }
}
