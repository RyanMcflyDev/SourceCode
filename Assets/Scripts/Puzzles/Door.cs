using UnityEngine;

// the blocker is on while closed, plates call Open and Close through their events
public class Door : MonoBehaviour
{
    [SerializeField] private GameObject blocker;
    [SerializeField] private bool startsOpen;

    private void Awake() => blocker.SetActive(!startsOpen);

    public void Open() => blocker.SetActive(false);
    public void Close() => blocker.SetActive(true);
}
