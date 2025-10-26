using UnityEngine;

public class PlayerMovementController : EntityComponent {
    [Header("References")]
    [SerializeField] private EntityCollider playerCollider;
    [SerializeField] private Transform groundCheckObject;
    [SerializeField] private Planet planet;

    [Header("Inputs")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftControl;
    [SerializeField] private KeyCode sneakKey = KeyCode.LeftShift;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float sneakSpeed = 2.1f;
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

    public bool isGrounded = false;
    public bool isMoving = false;
    public bool isSprinting = false;
    public bool isSneaking = false;
    public bool isCollidingOnMovement = false;

    private float speed = 0f;

    private float jumpHoldTimer = 0f;
    private float jumpHoldDelay = 0.5f;

    public override void OnEntityUpdate(bool ignoreLoops) {
        HandleInputs(ignoreLoops);
        HandleSneak(ignoreLoops);
        ApplyGravity();
        ApplyJump(ignoreLoops);
        ApplyMovements();
    }
    private void HandleInputs(bool ignoreLoops) {
        isGrounded = playerCollider.IsGrounded(planet, groundDistanceCheck);
        isSneaking = Input.GetKey(sneakKey);

        inputs = ignoreLoops ? Vector2.zero : new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        movement = Vector3.Lerp(movement, transform.right * inputs.x + transform.forward * inputs.y, isGrounded ? 14f : 5f * Time.deltaTime);

        isMoving = inputs != Vector2.zero;
        isSprinting = !isSneaking && isMoving && Input.GetKey(sprintKey) && !isCollidingOnMovement && inputs.y > 0f;
        
        speed = Mathf.Lerp(speed, isSneaking ? sneakSpeed : isSprinting ? sprintSpeed : walkSpeed, speedChangeMultiplier * Time.deltaTime);
    }
    private void HandleSneak(bool ignoreLoops) {
        if (ignoreLoops) return;

        playerCollider.height = isSneaking ? 1.5f : 1.8f;
    }

    private void ApplyGravity() {
        velocity.y += gravity * Time.deltaTime;

        if (isGrounded && velocity.y < 0f) {
            velocity.y = -2f;
        }
        else if (playerCollider.IsHeadHitting(planet, 0.05f)) {
            velocity.y = -2f;
        }
    }
    private bool IsEdgeBlocked(Vector3 direction) {
        if (!isGrounded || !isSneaking)
            return false;

        // small offset to "peek" ahead of the player’s collider
        Vector3 sneakOffset = direction * 0.01f + Vector3.down * 0.01f;

        // if there’s no solid voxel slightly below the next position, treat it as unsafe
        bool willFall = !playerCollider.CheckCollisions(planet, sneakOffset, out _);

        return willFall;
    }

    private void ApplyJump(bool ignoreLoops) {
        if (ignoreLoops) return;

        jumpHoldTimer -= Time.deltaTime;

        if (isGrounded) {
            if (Input.GetKeyDown(jumpKey)) {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpHoldTimer = jumpHoldDelay;
            }
            else if (Input.GetKey(jumpKey) && jumpHoldTimer <= 0f) {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpHoldTimer = jumpHoldDelay;
            }
        }
    }
    private void ApplyMovements() {
        Vector3 delta = movement * speed * Time.deltaTime;
        delta.y += velocity.y * Time.deltaTime;

        Vector3 newPosition = transform.position;

        // Offsets to attempt
        Vector3 fullOffset = new Vector3(delta.x, delta.y, delta.z);
        Vector3 offsetXY = new Vector3(delta.x, delta.y, 0f);
        Vector3 offsetZY = new Vector3(0f, delta.y, delta.z);
        Vector3 offsetX = new Vector3(delta.x, 0f, 0f);
        Vector3 offsetZ = new Vector3(0f, 0f, delta.z);
        Vector3 offsetY = new Vector3(0f, delta.y, 0f);

        // 1) If full move is possible, do it
        if (!playerCollider.CheckCollisions(planet, fullOffset, out _)) {
            // Sneak-edge checks must consider horizontal intent; prevent moving off edge if sneaking
            bool blockX = (isSneaking && delta.x != 0f) && IsEdgeBlocked(Vector3.right * Mathf.Sign(delta.x));
            bool blockZ = (isSneaking && delta.z != 0f) && IsEdgeBlocked(Vector3.forward * Mathf.Sign(delta.z));

            // If both X and Z would be blocked by sneak-edge, cancel horizontal components
            Vector3 applied = fullOffset;
            if (blockX) applied.x = 0f;
            if (blockZ) applied.z = 0f;

            // If we cancelled horizontal, re-check collisions for resulting offset (Y may still be valid)
            if (applied != fullOffset) {
                if (!playerCollider.CheckCollisions(planet, applied, out _)) {
                    newPosition += applied;
                    // zero out velocities for axes we blocked (horizontal)
                    if (blockX) velocity.x = 0f;
                    if (blockZ) velocity.z = 0f;
                    transform.position = newPosition;
                    return;
                }
                // fallthrough to combined attempts below
            }
            else {
                newPosition += fullOffset;
                transform.position = newPosition;
                return;
            }
        }

        // 2) Try X+Y (useful for stepping up) — but respect sneak-edge blocking for X
        bool triedXY = false;
        if (Mathf.Abs(offsetXY.x) > 0f) {
            bool blockX = isSneaking && IsEdgeBlocked(Vector3.right * Mathf.Sign(offsetXY.x));
            if (!blockX) {
                triedXY = true;
                if (!playerCollider.CheckCollisions(planet, offsetXY, out _)) {
                    newPosition += offsetXY;
                    transform.position = newPosition;
                    return;
                }
            }
        }

        // 3) Try Z+Y
        bool triedZY = false;
        if (Mathf.Abs(offsetZY.z) > 0f) {
            bool blockZ = isSneaking && IsEdgeBlocked(Vector3.forward * Mathf.Sign(offsetZY.z));
            if (!blockZ) {
                triedZY = true;
                if (!playerCollider.CheckCollisions(planet, offsetZY, out _)) {
                    newPosition += offsetZY;
                    transform.position = newPosition;
                    return;
                }
            }
        }

        // 4) If combined attempts failed, try per-axis moves (X, Z, then Y).
        // X
        if (Mathf.Abs(offsetX.x) > 0f) {
            bool blockX = isSneaking && IsEdgeBlocked(Vector3.right * Mathf.Sign(offsetX.x));
            if (!blockX && !playerCollider.CheckCollisions(planet, offsetX, out _)) {
                newPosition += offsetX;
            }
            else {
                velocity.x = 0f;
            }
        }

        // Z
        if (Mathf.Abs(offsetZ.z) > 0f) {
            bool blockZ = isSneaking && IsEdgeBlocked(Vector3.forward * Mathf.Sign(offsetZ.z));
            if (!blockZ && !playerCollider.CheckCollisions(planet, offsetZ, out _)) {
                newPosition += offsetZ;
            }
            else {
                velocity.z = 0f;
            }
        }

        // Y last: try to move vertically (if it collides, stop vertical velocity)
        if (!playerCollider.CheckCollisions(planet, offsetY, out _)) {
            newPosition += offsetY;
        }
        else {
            velocity.y = 0f;
        }

        transform.position = newPosition;
    }

}
