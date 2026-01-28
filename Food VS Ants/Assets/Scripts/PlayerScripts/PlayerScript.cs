using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerScript : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private PlayerInput _playerInput;

    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;

    [Header("Look Settings")]
    [SerializeField] private Transform _cameraTarget; // Empty GameObject for camera to follow
    [SerializeField] private float _lookSensitivity = 1f;
    [SerializeField] private float _maxLookAngle = 80f;

    [Header("UI References")]
    [SerializeField] private MovesetUIPanel _movesetUIPanel;

    private CharacterController _characterController;
    private InputAction _moveAction;
    private InputAction _lookAction;
    private Vector2 _lookInput;
    private float _cameraPitch = 0f;
    private LayerMask _foodGuardianLayer; // layer mask to ignore ant detection layer

    // Start is called before the first frame update
    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _moveAction = _playerInput.actions["Movement"];
        _lookAction = _playerInput.actions["Look"];

        // ignore FoodGuardian Layer/Box collider by using ignore raycast layer
        _foodGuardianLayer = ~LayerMask.GetMask("FoodGuardian");

        // lock and hide cursor when game starts
        LockCursor();
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleCursorState();
    }

    void HandleMovement()
    {
        Vector2 input = _moveAction.ReadValue<Vector2>();

        // Calculate movement direction relative to player rotation
        Vector3 move = transform.right * input.x + transform.forward * input.y;

        // Apply movement
        _characterController.Move(move * _moveSpeed * Time.deltaTime);

        // Apply gravity
        _characterController.Move(Vector3.down * 9.81f * Time.deltaTime);
    }

    void HandleCursorState()
    {
        // check if moveset panel is open
        bool isPanelOpen = _movesetUIPanel != null && _movesetUIPanel.IsPanelOpen();

        // if panel is open OR holding LeftAlt, unlock cursor
        if (isPanelOpen || Input.GetKey(KeyCode.LeftAlt))
        {
            UnlockCursor();
        }
        else
        {
            // lock cursor and handle look
            LockCursor();
            HandleLook();
        }
    }
    void HandleLook()
    {
        _lookInput = _lookAction.ReadValue<Vector2>();

        // Horizontal rotation (rotate player body)
        transform.Rotate(Vector3.up * _lookInput.x * _lookSensitivity);

        // Vertical rotation (rotate camera target)
        _cameraPitch -= _lookInput.y * _lookSensitivity;
        _cameraPitch = Mathf.Clamp(_cameraPitch, -_maxLookAngle, _maxLookAngle);

        if (_cameraTarget != null)
        {
            _cameraTarget.localEulerAngles = new Vector3(_cameraPitch, 0f, 0f);
        }
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
