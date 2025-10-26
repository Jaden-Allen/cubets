using UnityEngine;

public class PlayerMovementController : EntityComponent {
    [Header("References")]
    [SerializeField] private BoxVolumeCollider playerCollider;
    [SerializeField] private Transform groundCheckObject;
    [SerializeField] private Planet planet;

    [Header("Inputs")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftControl;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float speedChangeMultiplier = 7f;

    [Header("Gravity")]
    [SerializeField] private float groundDistanceCheck = 0.1f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Layers")]
    [SerializeField] private LayerMask groundMask;

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

        // --- Check if grounded ---
        isGrounded = playerCollider.IsGrounded(planet, groundDistanceCheck);

        // --- Movement state ---
        isMoving = inputs != Vector2.zero;
        isSprinting = isMoving && Input.GetKey(sprintKey);

        // --- Smooth speed change ---
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
        Vector3 delta = movement * speed * Time.deltaTime;
        delta.y += velocity.y * Time.deltaTime;

        Vector3 newPosition = transform.position;

        Vector3 offsetX = new Vector3(delta.x, 0f, 0f);
        if (!playerCollider.CheckCollisions(planet, offsetX, out Vector3 pushX)) {
            newPosition += offsetX;
        }
        else {
            velocity.x = 0f;
        }

        Vector3 offsetZ = new Vector3(0f, 0f, delta.z);
        if (!playerCollider.CheckCollisions(planet, offsetZ, out Vector3 pushZ)) {
            newPosition += offsetZ;
        }
        else {
            velocity.z = 0f;
        }

        Vector3 offsetY = new Vector3(0f, delta.y, 0f);
        if (!playerCollider.CheckCollisions(planet, offsetY, out Vector3 pushY)) {
            newPosition += offsetY;
        }
        else {
            velocity.y = 0f;
        }

        transform.position = newPosition;
    }

    private bool CollidesAt(Vector3 position) {
        // Player size
        float halfWidth = 0.3f;
        float height = 1.8f;

        // Check all voxels that the player would occupy
        for (int x = Mathf.FloorToInt(position.x - halfWidth); x <= Mathf.FloorToInt(position.x + halfWidth); x++) {
            for (int y = Mathf.FloorToInt(position.y); y <= Mathf.FloorToInt(position.y + height); y++) {
                for (int z = Mathf.FloorToInt(position.z - halfWidth); z <= Mathf.FloorToInt(position.z + halfWidth); z++) {
                    uint voxelIndex = planet.GetVoxel(new Vector3Int(x, y, z));

                    if (voxelIndex == BlockTypes.Water.registryIndex) continue;
                    if (voxelIndex != 0) return true;
                }
            }
        }
        return false;
    }


}
