using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerCharacterControllerMovement : EntityComponent
{
    [Header("References")]
    [SerializeField] private CharacterController characterController;

    [Header("Inputs")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftControl;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float speedChangeMultiplier = 7f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -9.81f;

    private Vector2 inputs = Vector3.zero;
    private Vector3 movement = Vector3.zero;
    private Vector3 velocity = Vector3.zero;

    private bool isGrounded = false;
    private bool isMoving = false;
    private bool isSprinting = false;

    private float speed = 0f;

    public override void OnEntityUpdate(bool ignoreLoops) {
        HandleInputs(ignoreLoops);
        ApplyGravity();
        ApplyJump(ignoreLoops);
        ApplyMovements();
    }
    private void HandleInputs(bool ignoreLoops) {
        inputs = ignoreLoops ? Vector2.zero : new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        movement = transform.right * inputs.x + transform.forward * inputs.y;
        isGrounded = characterController.isGrounded;
        isMoving = inputs != Vector2.zero;
        isSprinting = isMoving && Input.GetKey(sprintKey);

        speed = Mathf.Lerp(speed, isSprinting ? sprintSpeed : walkSpeed, speedChangeMultiplier * Time.deltaTime);
    }
    private void ApplyGravity() {
        velocity.y += gravity * Time.deltaTime;

        if (isGrounded && velocity.y < 0f) {
            velocity.y = -2f;
        }
    }
    private void ApplyJump(bool ignoreLoops) {
        if (!ignoreLoops && isGrounded && Input.GetKey(jumpKey)) {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }
    private void ApplyMovements() {
        characterController.Move(speed * Time.deltaTime * movement);
        characterController.Move(velocity * Time.deltaTime);
    }
}
