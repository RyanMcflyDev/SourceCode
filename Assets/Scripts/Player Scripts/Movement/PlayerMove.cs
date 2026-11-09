using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour, IResettable
{
    [SerializeField] private MovementSettings settings;
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private Transform cameraTransform;

    private const float GroundedPush = -2f;

    private CharacterController controller;
    // horizontal velocity from input and move speed
    private Vector3 horizontalVelocity;
    // gravity and vertical velocity
    private float verticalVelocity;
    // is byte jumping or falling
    private bool isLaunched;

    public MovementSettings Settings => settings;
    public bool IsGrounded => controller.isGrounded;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cameraTransform == null) cameraTransform = Camera.main.transform;
    }

    private void OnEnable() => moveAction.action.Enable();
    private void OnDisable() => moveAction.action.Disable();

    private void Update()
    {
        if (!isLaunched) UpdateWalking();
        UpdateGravity();
        UpdateFacing();

        controller.Move((horizontalVelocity + Vector3.up * verticalVelocity) * Time.deltaTime);

        if (isLaunched && controller.isGrounded) isLaunched = false;
    }

    private void UpdateWalking()
    {
        Vector2 input = moveAction.action.ReadValue<Vector2>();
        Vector3 targetVelocity = CameraRelative(input) * settings.MoveSpeed;

        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, settings.Acceleration * Time.deltaTime);
    }

    private void UpdateGravity()
    {
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            // a small constant push keeps isGrounded reliable on stairs and slopes
            verticalVelocity = GroundedPush;
            return;
        }

        verticalVelocity = Mathf.Max(verticalVelocity + settings.Gravity * Time.deltaTime, settings.TerminalVelocity);
    }

    private void UpdateFacing()
    {
        if (horizontalVelocity.sqrMagnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(horizontalVelocity);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, settings.TurnSpeed * Time.deltaTime);
    }

    private Vector3 CameraRelative(Vector2 input)
    {
        Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
        return forward * input.y + right * input.x;
    }

    public void Launch(Vector3 newHorizontalVelocity, float newVerticalVelocity)
    {
        horizontalVelocity = newHorizontalVelocity;
        verticalVelocity = newVerticalVelocity;
        isLaunched = true;
    }

    // stop byte keeping his falling speed after respawning
    public void ResetState()
    {
        horizontalVelocity = Vector3.zero;
        verticalVelocity = 0f;
        isLaunched = false;
    }
}
