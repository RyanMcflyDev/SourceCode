using UnityEngine;

// switches a hint on (a glitch line with a gc2 on enable trigger) after byte has been killed by one particular enemy
// plays once per level load, it doesn't come back on later deaths
public class HintOnDeath : MonoBehaviour
{
    [SerializeField] private GameObject killer;
    [SerializeField] private GameObject hint;
    [SerializeField] private int deathsBeforeHint = 1;

    private int deaths;
    private bool shown;

    private void OnEnable()
    {
        GameEvents.PlayerDied += OnPlayerDied;
        GameEvents.PlayerRespawned += OnPlayerRespawned;
    }

    private void OnDisable()
    {
        GameEvents.PlayerDied -= OnPlayerDied;
        GameEvents.PlayerRespawned -= OnPlayerRespawned;
    }

    // enemies pass their own name as the cause
    private void OnPlayerDied(string cause)
    {
        if (killer != null && cause == killer.name) deaths++;
    }

    // waits for the respawn so the line doesn't play over the death glitch
    private void OnPlayerRespawned()
    {
        if (shown || deaths < deathsBeforeHint) return;
        shown = true;
        hint.SetActive(true);
    }
}
