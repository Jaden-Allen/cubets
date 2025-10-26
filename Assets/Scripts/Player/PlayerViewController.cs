using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerViewController : EntityComponent
{
    [Header("References")]
    [SerializeField] private Transform playerBody;
    [SerializeField] private Camera playerCamera;

    [Header("Settings")]
    [SerializeField] private float mouseSensitivity = 1000f;
    [SerializeField] private float minHeadRotation = -90f;
    [SerializeField] private float maxHeadRotation = 90f;
    [SerializeField] private float standEyeHeight = 1.62f;
    [SerializeField] private float sneakEyeHeight = 1.27f;
    [SerializeField] private float fov = 90f;

    private float xRot = 0f;

    private Vector2 mouseInputs = Vector2.zero;
    private PlayerMovementController movementController;

    private float fovMultiplier = 1f;
    private float speedFovMultiplier = 0f;
    private float effectFovMultiplier = 0f;

    public override void OnEntityAwake() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public override void OnEntityStart() {
        movementController = entity.GetPlayerComponent<PlayerMovementController>();
    }

    public override void OnEntityUpdate(bool ignoreLoops) {
        if (ignoreLoops) return;
        
        HandleInputs();
        HandleFovEffects();
        HandleCameraPosition();
        ApplyRotations();
    }
    private void HandleInputs() {
        mouseInputs = new Vector2(
            Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime, 
            Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime);

        xRot -= mouseInputs.y;
        xRot = Mathf.Clamp(xRot, minHeadRotation, maxHeadRotation);
    }
    private void HandleFovEffects() {
        fovMultiplier = 1f;
        if (movementController.isSprinting) fovMultiplier += 0.15f;

        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, fov * fovMultiplier, 12f * Time.deltaTime);
    }
    private void HandleCameraPosition() {
        float targetEyeHeight = movementController.isSneaking ? sneakEyeHeight : standEyeHeight;
        playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, new Vector3(0f, targetEyeHeight, 0f), 12f * Time.deltaTime);
    }
    private void ApplyRotations() {
        playerBody.Rotate(0f, mouseInputs.x, 0f);
        playerCamera.transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);
    }
}
