using UnityEngine;

// turns byte's controls off for cutscenes and dialogue, gc2 can call Lock and Unlock with Invoke Method
public class PlayerLock : MonoBehaviour
{
    [SerializeField] private Behaviour[] controls;

    // remembers what was on so unlocking doesn't switch on things a level turned off (code vision)
    private bool[] wasEnabled;

    public bool IsLocked { get; private set; }

    public void Lock()
    {
        if (IsLocked) return;
        IsLocked = true;

        wasEnabled = new bool[controls.Length];
        for (int i = 0; i < controls.Length; i++)
        {
            wasEnabled[i] = controls[i].enabled;
            // stops byte sliding on with his old speed when unlocked
            if (controls[i] is IResettable resettable) resettable.ResetState();
            controls[i].enabled = false;
        }

        // the animator reads the controller's last move speed, a zero move drops it to idle
        if (TryGetComponent(out CharacterController controller)) controller.Move(Vector3.zero);
    }

    public void Unlock()
    {
        if (!IsLocked) return;
        IsLocked = false;

        for (int i = 0; i < controls.Length; i++)
            controls[i].enabled = wasEnabled[i];
    }
}
