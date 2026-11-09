using UnityEngine;

// turns objects on or off when byte swaps with the target
public class ToggleOnSwap : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private Transform target;

    [Header("Objects")]
    [SerializeField] private GameObject[] turnOff;
    [SerializeField] private GameObject[] turnOn;

    private void OnEnable() => GameEvents.PlayerSwapped += OnPlayerSwapped;
    private void OnDisable() => GameEvents.PlayerSwapped -= OnPlayerSwapped;

    private void OnPlayerSwapped(Transform swappedWith)
    {
        if (swappedWith != target) return;

        SetAll(turnOff, false);
        SetAll(turnOn, true);
    }

    private static void SetAll(GameObject[] objects, bool active)
    {
        foreach (var obj in objects)
            if (obj != null) obj.SetActive(active);
    }
}
