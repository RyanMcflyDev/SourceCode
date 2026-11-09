using UnityEngine;
using UnityEngine.InputSystem;

public class CodeVision : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference codeVisionAction;

    [Header("Timing")]
    // 0 means it stays on until pressed again
    [SerializeField] private float duration = 0f;

    private float endTime;

    public bool IsActive { get; private set; }

    private void OnEnable()
    {
        codeVisionAction.action.Enable();
        codeVisionAction.action.performed += OnToggle;
        GameEvents.PlayerDied += OnPlayerDied;
    }

    private void OnDisable()
    {
        codeVisionAction.action.performed -= OnToggle;
        codeVisionAction.action.Disable();
        GameEvents.PlayerDied -= OnPlayerDied;
    }

    private void Update()
    {
        if (IsActive && duration > 0f && Time.unscaledTime >= endTime)
            SetActive(false);
    }

    private void OnToggle(InputAction.CallbackContext context) => SetActive(!IsActive);

    // turn it off on death so the room comes back looking normal
    private void OnPlayerDied(string cause) => SetActive(false);

    private void SetActive(bool active)
    {
        if (IsActive == active) return;

        IsActive = active;
        endTime = Time.unscaledTime + duration;
        GameEvents.RaiseCodeVisionChanged(active);
    }
}
