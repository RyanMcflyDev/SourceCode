using UnityEngine;

// switches other objects off while this one is on, and back on when it goes off
// e.g. on the intro's code rift, so the grass lid over the code hole goes when the rift opens
public class HideWhileActive : MonoBehaviour
{
    [SerializeField] private GameObject[] hide;

    private void OnEnable() => SetHidden(true);
    private void OnDisable() => SetHidden(false);

    private void SetHidden(bool hidden)
    {
        foreach (GameObject target in hide)
            if (target != null) target.SetActive(!hidden);
    }
}
