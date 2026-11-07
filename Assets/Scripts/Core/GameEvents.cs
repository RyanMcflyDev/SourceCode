using System;
using UnityEngine;

public static class GameEvents
{
    public static event Action<string> PlayerDied;
    public static event Action PlayerRespawned;
    public static event Action<Transform> SpawnPointChanged;
    public static event Action<bool> CodeVisionChanged;
    public static event Action<Transform> PlayerSwapped;

    // so things that switch on mid code vision can check if it's already active
    public static bool CodeVisionActive { get; private set; }

    public static void RaisePlayerDied(string cause) => PlayerDied?.Invoke(cause);
    public static void RaisePlayerRespawned() => PlayerRespawned?.Invoke();
    public static void RaiseSpawnPointChanged(Transform spawnPoint) => SpawnPointChanged?.Invoke(spawnPoint);
    public static void RaisePlayerSwapped(Transform target) => PlayerSwapped?.Invoke(target);

    public static void RaiseCodeVisionChanged(bool active)
    {
        CodeVisionActive = active;
        CodeVisionChanged?.Invoke(active);
    }

// clear old listeners when entering play mode in case domain reload is off
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ClearOnPlay()
    {
        PlayerDied = null;
        PlayerRespawned = null;
        SpawnPointChanged = null;
        CodeVisionChanged = null;
        PlayerSwapped = null;
        CodeVisionActive = false;
    }
}
