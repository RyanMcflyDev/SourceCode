using UnityEngine;

public class EnemyDeath : MonoBehaviour, IKillable, IResettable
{
    private Rigidbody _body;

    public bool IsDead { get; private set; }

    private void Awake() => _body = GetComponent<Rigidbody>();

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
        if (_body == null) return;

        // stop any fall speed then go back to being moved by the navmesh agent
        if (!_body.isKinematic)
        {
            _body.linearVelocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;
        }
        _body.isKinematic = true;
    }
}
