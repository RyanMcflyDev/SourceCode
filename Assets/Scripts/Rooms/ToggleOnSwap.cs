using UnityEngine;

// turns objects on or off when byte swaps with the target
public class ToggleOnSwap : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private Transform _target;

    [Header("Objects")]
    [SerializeField] private GameObject[] _turnOff;
    [SerializeField] private GameObject[] _turnOn;

    private void OnEnable() => GameEvents.PlayerSwapped += OnPlayerSwapped;
    private void OnDisable() => GameEvents.PlayerSwapped -= OnPlayerSwapped;

    private void OnPlayerSwapped(Transform swappedWith)
    {
        if (swappedWith != _target) return;

        SetAll(_turnOff, false);
        SetAll(_turnOn, true);
    }

    private static void SetAll(GameObject[] objects, bool active)
    {
        foreach (var obj in objects)
            if (obj != null) obj.SetActive(active);
    }
}
