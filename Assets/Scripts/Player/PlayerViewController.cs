using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerViewController : EntityComponent
{
    [Header("References")]
    [SerializeField] private Transform playerBody;
    [SerializeField] private Transform playerCamera;

    [Header("Settings")]
    [SerializeField] private float mouseSensitivity = 1000f;
    [SerializeField] private float minHeadRotation = -90f;
    [SerializeField] private float maxHeadRotation = 90f;

    private float xRot = 0f;

    private Vector2 mouseInputs = Vector2.zero;

    public override void OnEntityAwake() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void OnEntityUpdate(bool ignoreLoops) {
        if (ignoreLoops) return;
        
        HandleInputs();
        ApplyRotations();
    }
    private void HandleInputs() {
        mouseInputs = new Vector2(
            Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime, 
            Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime);

        xRot -= mouseInputs.y;
        xRot = Mathf.Clamp(xRot, minHeadRotation, maxHeadRotation);
    }
    private void ApplyRotations() {
        playerBody.Rotate(0f, mouseInputs.x, 0f);
        playerCamera.localRotation = Quaternion.Euler(xRot, 0f, 0f);
    }
}
