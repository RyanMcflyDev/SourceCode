using UnityEngine;

[RequireComponent(typeof(Collider))]
public class KillVolume : MonoBehaviour
{
    [SerializeField] private string _cause = "pit";

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        var killable = other.GetComponentInParent<IKillable>();
        if (killable != null) killable.Kill(_cause);
    }
}
