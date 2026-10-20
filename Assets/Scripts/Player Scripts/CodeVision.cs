using UnityEngine;
using UnityEngine.InputSystem;

public class CodeVision : MonoBehaviour
{
    [Header("input")]
    [SerializeField] private InputActionReference _codeVisionAction;

    [Header("Timing")]
    // 0 means it stays on until pressed again
    [SerializeField] private float _duration = 0f;

    private bool _isActive;
    private float _endTime;

    public bool IsActive => _isActive;

    private void OnEnable()
    {
        _codeVisionAction.action.Enable();
        _codeVisionAction.action.performed += OnToggle;
        GameEvents.PlayerDied += OnPlayerDied;
    }

    private void OnDisable()
    {
        _codeVisionAction.action.performed -= OnToggle;
        _codeVisionAction.action.Disable();
        GameEvents.PlayerDied -= OnPlayerDied;
    }

    private void Update()
    {
        if (_isActive && _duration > 0f && Time.unscaledTime >= _endTime)
            SetActive(false);
    }

    private void OnToggle(InputAction.CallbackContext context) => SetActive(!_isActive);

    // turn it off on death so the room comes back looking normal
    private void OnPlayerDied(string cause) => SetActive(false);

    private void SetActive(bool active)
    {
        if (_isActive == active) return;

        _isActive = active;
        _endTime = Time.unscaledTime + _duration;
        GameEvents.RaiseCodeVisionChanged(active);
    }
}
