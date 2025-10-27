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

        Vector3 stepCheck = direction.normalized * Mathf.Max(speed * Time.deltaTime, 0.05f);
        Vector3[] corners = playerCollider.GetHorizontalCorners(transform.position + stepCheck);

        int groundedCorners = 0;

        foreach (var corner in corners) {
            Vector3Int below = Vector3Int.FloorToInt(corner + Vector3.down * 0.1f);
            Block block = planet.GetBlock(below);
            if (block != null && block.blockData.collision.enabled)
                groundedCorners++;
        }

        return groundedCorners == 0;
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
        float stepHeight = 0.5f;

        Vector3 fullOffset = new Vector3(delta.x, delta.y, delta.z);
        Vector3 offsetX = new Vector3(delta.x, 0f, 0f);
        Vector3 offsetZ = new Vector3(0f, 0f, delta.z);
        Vector3 offsetY = new Vector3(0f, delta.y, 0f);

        if (!playerCollider.CheckCollisions(planet, fullOffset)) {
            newPosition += fullOffset;
            transform.position = newPosition;
            return;
        }

        bool moved = false;

        if (Mathf.Abs(offsetX.x) > 0f) {
            bool blockX = isSneaking && IsEdgeBlocked(Vector3.right * Mathf.Sign(offsetX.x));
            if (!blockX) {
                if (!playerCollider.CheckCollisions(planet, offsetX)) {
                    newPosition += offsetX;
                    moved = true;
                }
                else {
                    Vector3 stepUp = Vector3.up * stepHeight;
                    if (!playerCollider.CheckCollisions(planet, stepUp) &&
                        !playerCollider.CheckCollisions(planet, stepUp + offsetX)) {
                        newPosition += stepUp + offsetX;
                        moved = true;
                    }
                    else {
                        velocity.x = 0f;
                    }
                }
            }
        }

        if (Mathf.Abs(offsetZ.z) > 0f) {
            bool blockZ = isSneaking && IsEdgeBlocked(Vector3.forward * Mathf.Sign(offsetZ.z));
            if (!blockZ) {
                if (!playerCollider.CheckCollisions(planet, offsetZ)) {
                    newPosition += offsetZ;
                    moved = true;
                }
                else {
                    Vector3 stepUp = Vector3.up * stepHeight;
                    if (!playerCollider.CheckCollisions(planet, stepUp) &&
                        !playerCollider.CheckCollisions(planet, stepUp + offsetZ)) {
                        newPosition += stepUp + offsetZ;
                        moved = true;
                    }
                    else {
                        velocity.z = 0f;
                    }
                }
            }
        }

        if (!playerCollider.CheckCollisions(planet, offsetY)) {
            newPosition += offsetY;
        }
        else {
            velocity.y = 0f;
        }

        if (moved || newPosition != transform.position) {
            transform.position = newPosition;
        }
    }


}
