using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMove))]

public class PlayerJump : MonoBehaviour
{
    
    [SerializeField] private InputActionReference _jumpAction;

    private PlayerMove _move;

    private void Awake() => _move = GetComponent<PlayerMove>();

    private void OnEnable()
    {
        _jumpAction.action.Enable();
        _jumpAction.action.performed += OnJump;
    }

    private void OnDisable()
    {
        _jumpAction.action.performed -= OnJump;
        _jumpAction.action.Disable();
    }

    private void OnJump(InputAction.CallbackContext context) => TryJump();

    private void TryJump()
    {
        if (!_move.IsGrounded) return;

        // Fixed arc: the same distance every time, whatever speed Byte was moving at
        float gravity = -_move.Settings.Gravity;
        float upSpeed = Mathf.Sqrt(2f * gravity * _move.Settings.JumpHeight);
        float airTime = 2f * upSpeed / gravity;
        float forwardSpeed = _move.Settings.JumpDistance / airTime;

        _move.Launch(transform.forward * forwardSpeed, upSpeed);
    
    
 
    
    
    

   


}

}
