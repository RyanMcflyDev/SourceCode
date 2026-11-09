using UnityEngine;

// roof pane that hides the rest of the level until the player swaps with something tagged to reveal it
public class FogOfWar : MonoBehaviour
{
    [SerializeField] private string revealTag = "FogOfWar";
    // only these objects clear this fog, leave empty so any object with the reveal tag clears it
    [SerializeField] private Transform[] revealers;

    private void OnEnable() => GameEvents.PlayerSwapped += OnPlayerSwapped;
    private void OnDisable() => GameEvents.PlayerSwapped -= OnPlayerSwapped;

    private void OnPlayerSwapped(Transform target)
    {
        if (!target.CompareTag(revealTag)) return;
        if (revealers != null && revealers.Length > 0 && System.Array.IndexOf(revealers, target) < 0) return;

        gameObject.SetActive(false);
    }
}
