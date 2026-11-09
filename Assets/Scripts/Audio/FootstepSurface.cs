using System.Collections.Generic;
using UnityEngine;

// put on a floor for its footstep sound, or give it a trigger box zone to override the floor inside that box
public class FootstepSurface : MonoBehaviour
{
    [SerializeField] private FootstepSet footsteps;
    [SerializeField] private BoxCollider zone;

    private static readonly List<FootstepSurface> Zones = new List<FootstepSurface>();

    public FootstepSet Footsteps => footsteps;

    private void OnEnable()
    {
        if (zone != null) Zones.Add(this);
    }

    private void OnDisable() => Zones.Remove(this);

    public static FootstepSet FindZone(Vector3 point)
    {
        foreach (var surface in Zones)
            if (surface.zone.bounds.Contains(point)) return surface.footsteps;

        return null;
    }

    // domain reload is off, so clear zones left over from the last play
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ClearOnPlay() => Zones.Clear();
}
