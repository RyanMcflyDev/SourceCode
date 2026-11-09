using UnityEngine;
using UnityEngine.Events;

// pressed while anything is in the press area, doesn't latch so its door closes again when it's empty
public class PressurePlate : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private BoxCollider pressArea;
    [SerializeField] private LayerMask pressLayers;
    [SerializeField] private bool byteCanPress = true;

    [Header("Visual")]
    [SerializeField] private Transform plateTop;
    [SerializeField] private float pressDepth = 0.05f;

    [Header("Events")]
    [SerializeField] private UnityEvent onPressed;
    [SerializeField] private UnityEvent onReleased;
    // first time a box or enemy presses it, byte doesn't count, resets on death
    [SerializeField] private UnityEvent onFirstObjectPress;

    private bool isPressed;
    private bool objectPressedBefore;

    private void OnEnable() => GameEvents.PlayerDied += OnPlayerDied;
    private void OnDisable() => GameEvents.PlayerDied -= OnPlayerDied;

    private void OnPlayerDied(string cause) => objectPressedBefore = false;

    // checked every physics step instead of trigger events, swaps teleport things and dead enemies switch off without an exit
    private void FixedUpdate()
    {
        bool byteOn = false;
        bool objectOn = false;

        foreach (Collider hit in Physics.OverlapBox(pressArea.bounds.center, pressArea.bounds.extents, Quaternion.identity, pressLayers, QueryTriggerInteraction.Ignore))
        {
            if (hit.CompareTag("Player")) byteOn = true;
            else objectOn = true;
        }

        if (objectOn && !objectPressedBefore)
        {
            objectPressedBefore = true;
            onFirstObjectPress.Invoke();
        }

        bool pressed = objectOn || (byteOn && byteCanPress);
        if (pressed == isPressed) return;

        isPressed = pressed;
        plateTop.position += Vector3.down * (pressed ? pressDepth : -pressDepth);

        if (pressed) onPressed.Invoke();
        else onReleased.Invoke();
    }
}
