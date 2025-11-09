using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private PlayerInput _playerInput;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Look Settings")]
    [SerializeField] private Transform cameraTarget; // Empty GameObject for camera to follow
    [SerializeField] private float lookSensitivity = 1f;
    [SerializeField] private float maxLookAngle = 80f;

    private CharacterController _characterController;
    private InputAction _moveAction;
    private InputAction _lookAction;
    private Vector2 _lookInput;
    private float _cameraPitch = 0f;

    // Start is called before the first frame update
    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _moveAction = _playerInput.actions["Movement"];
        _lookAction = _playerInput.actions["Look"];

        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleLook();
    }

    void HandleMovement()
    {
        Vector2 input = _moveAction.ReadValue<Vector2>();

        // Calculate movement direction relative to player rotation
        Vector3 move = transform.right * input.x + transform.forward * input.y;

        // Apply movement
        _characterController.Move(move * moveSpeed * Time.deltaTime);

        // Apply gravity
        _characterController.Move(Vector3.down * 9.81f * Time.deltaTime);
    }

    void HandleLook()
    {
        _lookInput = _lookAction.ReadValue<Vector2>();

        // Horizontal rotation (rotate player body)
        transform.Rotate(Vector3.up * _lookInput.x * lookSensitivity);

        // Vertical rotation (rotate camera target)
        _cameraPitch -= _lookInput.y * lookSensitivity;
        _cameraPitch = Mathf.Clamp(_cameraPitch, -maxLookAngle, maxLookAngle);

        if (cameraTarget != null)
        {
            cameraTarget.localEulerAngles = new Vector3(_cameraPitch, 0f, 0f);
        }
    }

    /*
    Create Camera Target:

Create an empty GameObject as a child of your Player
Position it at eye level (around Y = 1.6)
Name it "CameraTarget"
Drag it to the cameraTarget field in the script


Setup Cinemachine Virtual Camera:

In Unity: GameObject > Cinemachine > Virtual Camera
Set Follow to your Player GameObject
Set Look At to the CameraTarget
In the Body section, choose "3rd Person Follow" or "Framing Transposer"
Set Camera Distance to 0 (or very close) for first-person
In Aim section, choose "Same As Follow Target"


Input Actions Setup:

Open your Input Actions asset
Make sure you have these actions:

Move: Action Type = "Value", Control Type = "Vector2", bound to WASD/Left Stick
Look: Action Type = "Value", Control Type = "Vector2", bound to Mouse Delta/Right Stick




Player Input Component:

Make sure your PlayerInput component references your Input Actions asset
Set Behavior to "Send Messages" or "Invoke Unity Events"



Would you like me to adjust anything, like adding jumping, sprinting, or different camera styles?RetryClaude can make mistakes. Please double-check responses.
    */
}
