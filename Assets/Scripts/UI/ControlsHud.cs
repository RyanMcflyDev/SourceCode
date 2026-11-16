using UnityEngine;
using UnityEngine.InputSystem;

// controls list in the corner, H hides it, the code vision line only shows once code vision is unlocked
public class ControlsHud : MonoBehaviour
{
    [SerializeField] private InputAction toggleAction = new InputAction("Toggle Controls", InputActionType.Button, "<Keyboard>/h");
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject codeVisionLine;

    [Header("New Line Highlight")]
    [SerializeField] private CanvasGroup newHighlight;
    [SerializeField] private float highlightTime = 6f;
    [SerializeField] private float pulseSpeed = 6f;

    private CodeVision codeVision;
    private float highlightUntil = -1f;

    private void Awake() => codeVision = FindAnyObjectByType<CodeVision>();

    private void OnEnable()
    {
        toggleAction.Enable();
        toggleAction.performed += OnToggle;
    }

    // domain reload is off so the action has to be turned off here
    private void OnDisable()
    {
        toggleAction.performed -= OnToggle;
        toggleAction.Disable();
    }

    private void OnToggle(InputAction.CallbackContext context) => panel.SetActive(!panel.activeSelf);

    private void Update()
    {
        // levels switch code vision on and off, so check it rather than set it once
        bool unlocked = codeVision != null && codeVision.enabled;
        if (codeVisionLine.activeSelf != unlocked)
        {
            codeVisionLine.SetActive(unlocked);
            // the moment it unlocks, open the list even if it was hidden and make the new line hard to miss
            if (unlocked)
            {
                panel.SetActive(true);
                highlightUntil = Time.unscaledTime + highlightTime;
            }
        }

        if (newHighlight == null) return;
        float left = highlightUntil - Time.unscaledTime;
        if (left <= 0f)
        {
            newHighlight.alpha = 0f;
            return;
        }
        float pulse = 0.4f + 0.6f * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * pulseSpeed));
        newHighlight.alpha = pulse * Mathf.Clamp01(left / 0.5f);
    }
}
