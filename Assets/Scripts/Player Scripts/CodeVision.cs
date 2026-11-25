using UnityEngine;
using UnityEngine.InputSystem;

public class CodeVision : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference codeVisionAction;

    [Header("Timing")]
    // 0 means it stays on until pressed again
    [SerializeField] private float duration = 0f;

    [Header("Bullet Time")]
    [SerializeField] private bool bulletTime = false;
    [SerializeField] private float slowScale = 0.2f;
    // real seconds before it can be used again, the same for every preset
    [SerializeField] private float cooldown = 3f;
    // research presets, replace the duration above while bullet time is on
    [SerializeField] private float[] presetDurations = { 1.5f, 10f, 30f };

    // static so the chosen preset carries over between scenes and restarts
    public static int Preset { get; set; }

    private float timeLeft;
    private float readyTime;

    public bool IsActive { get; private set; }
    public bool HasBulletTime => bulletTime;
    public float CurrentDuration => bulletTime ? presetDurations[Preset] : duration;
    public int PresetCount => presetDurations.Length;

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

    // leaving the scene mid bullet time would keep the next scene slowed
    private void OnDestroy()
    {
        if (IsActive && bulletTime) Time.timeScale = 1f;
    }

    private void Update()
    {
        // real time so the slow motion doesn't stretch it, but not while paused
        if (!IsActive || CurrentDuration <= 0f || Time.timeScale == 0f) return;

        timeLeft -= Time.unscaledDeltaTime;
        if (timeLeft <= 0f) SetActive(false);
    }

    public float PresetDuration(int index) => presetDurations[index];

    private void OnToggle(InputAction.CallbackContext context)
    {
        if (!IsActive && bulletTime && Time.unscaledTime < readyTime) return;
        SetActive(!IsActive);
    }

    // turn it off on death so the room comes back looking normal, and ready again for the retry
    private void OnPlayerDied(string cause)
    {
        SetActive(false);
        readyTime = 0f;
    }

    private void SetActive(bool active)
    {
        if (IsActive == active) return;

        IsActive = active;
        timeLeft = CurrentDuration;

        if (bulletTime)
        {
            Time.timeScale = active ? slowScale : 1f;
            if (!active) readyTime = Time.unscaledTime + cooldown;
        }

        GameEvents.RaiseCodeVisionChanged(active);
    }
}
