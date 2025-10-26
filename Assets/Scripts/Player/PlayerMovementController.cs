using UnityEngine;

public class PlayerMovementController : EntityComponent {
    [Header("References")]
    [SerializeField] private BoxVolumeCollider playerCollider;
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
        inputs = ignoreLoops ? Vector2.zero : new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        movement = transform.right * inputs.x + transform.forward * inputs.y;

        isGrounded = playerCollider.IsGrounded(planet, groundDistanceCheck);
        
        isSneaking = Input.GetKey(sneakKey);
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
    private void ApplyJump(bool ignoreLoops) {
        if (ignoreLoops) return;

        jumpHoldTimer -= Time.deltaTime;

        if (isGrounded) {
            if (Input.GetKeyDown(jumpKey)) {
                // Immediate jump on press
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpHoldTimer = jumpHoldDelay;
            }
            else if (Input.GetKey(jumpKey) && jumpHoldTimer <= 0f) {
                // Automatic jump if holding after delay
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpHoldTimer = jumpHoldDelay;
            }
        }
    }
    private void ApplyMovements() {
        Vector3 delta = movement * speed * Time.deltaTime;
        delta.y += velocity.y * Time.deltaTime;

        Vector3 newPosition = transform.position;

        bool collisionX = false;
        bool collisionZ = false;

        Vector3 offsetX = new Vector3(delta.x, 0f, 0f);
        if (!playerCollider.CheckCollisions(planet, offsetX, out Vector3 pushX)) {
            newPosition += offsetX;
            collisionX = false;
        }
        else {
            velocity.x = 0f;
            collisionX = true;
        }

        Vector3 offsetZ = new Vector3(0f, 0f, delta.z);
        if (!playerCollider.CheckCollisions(planet, offsetZ, out Vector3 pushZ)) {
            newPosition += offsetZ;
            collisionZ = false;
        }
        else {
            velocity.z = 0f;
            collisionZ = true;
        }

        isCollidingOnMovement = collisionX || collisionZ;

        Vector3 offsetY = new Vector3(0f, delta.y, 0f);
        if (!playerCollider.CheckCollisions(planet, offsetY, out Vector3 pushY)) {
            newPosition += offsetY;
        }
        else {
            velocity.y = 0f;
        }

        transform.position = newPosition;
    }
}
