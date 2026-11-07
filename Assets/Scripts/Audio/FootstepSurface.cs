using System.Collections.Generic;
using UnityEngine;


public class FootstepSurface : MonoBehaviour
{
    [Header("Sound")]
    [SerializeField] private FootstepSet _footsteps;

    [Header("Zone")]
    // leave at zero to use this object's collider, or set a size to override the floor in that area
    [SerializeField] private Vector3 _zoneSize;

    private static readonly List<FootstepSurface> Zones = new List<FootstepSurface>();

    public FootstepSet Footsteps => _footsteps;

    private bool IsZone => _zoneSize != Vector3.zero;

    private void OnEnable()
    {
        if (IsZone) Zones.Add(this);
    }

    private void OnDisable() => Zones.Remove(this);

    public static FootstepSet FindZone(Vector3 point)
    {
        foreach (var zone in Zones)
            if (new Bounds(zone.transform.position, zone._zoneSize).Contains(point))
                return zone._footsteps;

        return null;
    }

    // domain reload is off, so clear zones left over from the last play
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ClearOnPlay() => Zones.Clear();

    private void OnDrawGizmosSelected()
    {
        if (!IsZone) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, _zoneSize);
    }
}
