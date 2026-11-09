using UnityEngine;

public class EnemyDeath : MonoBehaviour, IKillable, IResettable
{
    private Rigidbody body;

    public bool IsDead { get; private set; }

    private void Awake() => body = GetComponent<Rigidbody>();

    // dissolved = turned off, not destroyed, so the room reset can bring it back
    public void Kill(string cause)
    {
        if (IsDead) return;
        IsDead = true;

        Debug.Log($"[EnemyDeath] {gameObject.name} killed by {cause}");
        gameObject.SetActive(false);
    }

    public void ResetState()
    {
        IsDead = false;
        if (body == null) return;

        // stop any fall speed then go back to being moved by the navmesh agent
        if (!body.isKinematic)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
        body.isKinematic = true;
    }
}
