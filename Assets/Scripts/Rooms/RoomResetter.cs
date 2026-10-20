using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RoomResetter : MonoBehaviour
{
    [Header("Tracked Objects")]
    [SerializeField] private Transform _trackAllChildrenOf;
    [SerializeField] private Transform[] _alsoTrack;

    private struct Snapshot
    {
        public Transform Target;
        public Vector3 Position;
        public Quaternion Rotation;
    }

    private readonly List<Snapshot> _snapshots = new List<Snapshot>();

    private void Awake() // remember the starting positions of everything we want to reset
    {
        if (_trackAllChildrenOf != null)
            foreach (Transform child in _trackAllChildrenOf) Remember(child);

        if (_alsoTrack != null)
            foreach (var t in _alsoTrack) if (t != null) Remember(t);
    }

    private void OnEnable() => GameEvents.PlayerDied += OnPlayerDied;
    private void OnDisable() => GameEvents.PlayerDied -= OnPlayerDied;

    private void OnPlayerDied(string cause) => ResetRoom();

    private void Remember(Transform t)
    {
        _snapshots.Add(new Snapshot { Target = t, Position = t.position, Rotation = t.rotation });
    }

// put everything back where it started
    public void ResetRoom()
    {
        foreach (var snapshot in _snapshots)
        {
            // only null if something got destroyed, use SetActive(false) instead
            if (snapshot.Target == null) continue;
            Restore(snapshot.Target, snapshot.Position, snapshot.Rotation);
        }
    }

    private static void Restore(Transform t, Vector3 position, Quaternion rotation)
    {
        // killed enemies are disabled not destroyed, so turn them back on
        if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);

        if (t.TryGetComponent(out NavMeshAgent agent))
        {
            // swapper turns the agent off if it lands off the navmesh
            if (!agent.enabled) agent.enabled = true;
            agent.Warp(position);
            t.rotation = rotation;
        }
        else if (t.TryGetComponent(out CharacterController controller))
        {
            controller.enabled = false;
            t.SetPositionAndRotation(position, rotation);
            controller.enabled = true;
        }
        else
        {
            t.SetPositionAndRotation(position, rotation);

            if (t.TryGetComponent(out Rigidbody body) && !body.isKinematic)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
        }

        foreach (var resettable in t.GetComponents<IResettable>())
            resettable.ResetState();
    }
}
