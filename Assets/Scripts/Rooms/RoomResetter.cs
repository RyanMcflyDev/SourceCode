using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RoomResetter : MonoBehaviour
{
    [Header("Tracked Objects")]
    [SerializeField] private Transform trackAllChildrenOf;
    [SerializeField] private Transform[] alsoTrack;

    private struct Snapshot
    {
        public Transform Target;
        public Vector3 Position;
        public Quaternion Rotation;
    }

    private readonly List<Snapshot> snapshots = new List<Snapshot>();

    // remember the starting positions of everything we want to reset
    private void Awake()
    {
        if (trackAllChildrenOf != null)
            foreach (Transform child in trackAllChildrenOf) Remember(child);

        if (alsoTrack != null)
            foreach (var t in alsoTrack) if (t != null) Remember(t);
    }

    private void OnEnable() => GameEvents.PlayerDied += OnPlayerDied;
    private void OnDisable() => GameEvents.PlayerDied -= OnPlayerDied;

    private void OnPlayerDied(string cause) => ResetRoom();

    private void Remember(Transform t)
    {
        snapshots.Add(new Snapshot { Target = t, Position = t.position, Rotation = t.rotation });
    }

    // put everything back where it started
    public void ResetRoom()
    {
        foreach (var snapshot in snapshots)
        {
            // only null if something got destroyed, use SetActive(false) instead
            if (snapshot.Target == null) continue;
            Restore(snapshot.Target, snapshot.Position, snapshot.Rotation);
        }
    }

    // byte resets through PlayerDeath, so nothing here has a character controller
    private static void Restore(Transform t, Vector3 position, Quaternion rotation)
    {
        // killed enemies are disabled not destroyed, so turn them back on
        if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);

        if (t.TryGetComponent(out NavMeshAgent agent))
        {
            // swapper turns the agent off if it lands off the navmesh
            agent.enabled = true;
            agent.Warp(position);
            t.rotation = rotation;
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
