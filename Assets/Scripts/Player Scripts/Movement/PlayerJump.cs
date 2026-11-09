using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMove))]
public class PlayerJump : MonoBehaviour
{
    [SerializeField] private InputActionReference jumpAction;

    private PlayerMove move;

    private void Awake() => move = GetComponent<PlayerMove>();

    private void OnEnable()
    {
        jumpAction.action.Enable();
        jumpAction.action.performed += OnJump;
    }

    private void OnDisable()
    {
        jumpAction.action.performed -= OnJump;
        jumpAction.action.Disable();
    }

    private void OnJump(InputAction.CallbackContext context) => TryJump();

    private void TryJump()
    {
        if (!move.IsGrounded) return;

        // fixed arc, the same distance every time whatever speed byte was moving at
        float gravity = -move.Settings.Gravity;
        float upSpeed = Mathf.Sqrt(2f * gravity * move.Settings.JumpHeight);
        float airTime = 2f * upSpeed / gravity;
        float forwardSpeed = move.Settings.JumpDistance / airTime;

        move.Launch(transform.forward * forwardSpeed, upSpeed);
    }
}
