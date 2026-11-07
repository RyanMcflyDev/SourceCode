using UnityEngine;

// roof pane that hides the rest of the level until the player swaps with something tagged to reveal it
public class FogOfWar : MonoBehaviour
{
    [SerializeField] private string _revealTag = "FogOfWar";
    [Tooltip("Only these objects clear this fog. Leave empty so any object with the reveal tag clears it")]
    [SerializeField] private Transform[] _revealers;

    private void OnEnable() => GameEvents.PlayerSwapped += OnPlayerSwapped;
    private void OnDisable() => GameEvents.PlayerSwapped -= OnPlayerSwapped;

    private void OnPlayerSwapped(Transform target)
    {
        if (!target.CompareTag(_revealTag)) return;
        if (_revealers != null && _revealers.Length > 0 &&System.Array.IndexOf(_revealers, target) < 0) return;

        gameObject.SetActive(false);
    }
}
