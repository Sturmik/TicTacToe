using UnityEngine;

public class CameraControl : MonoBehaviour
{
    #region Variables

    // Field control manager to which events camera will subscribe
    [SerializeField] private FieldControlManager _fieldControlManagerScript;
    // Camera max and min Y angle
    private const float CAMERA_MAX_ANGLE = 40;
    private const float CAMERA_MIN_ANGLE = -40;

    // Camera movement constraints
    [SerializeField] private float _cameraMovementConstraintRadius;
    // Camera movement speed
    [SerializeField] private float _cameraMovementSpeed;
    // Camera default position and rotation
    private Vector3 _cameraDefaultPos;
    private Quaternion _cameraDefaultRotation;
    // Main camera
    [SerializeField] private GameObject _mainCamera;
    // Sensitivity of the mouse
    [SerializeField] private float _sensitivity;
    // Camera pitch
    private float _cameraPitchX;
    private float _cameraPitchY;
    // Can camera move
    private bool _canMove;

    #endregion

    #region Unity

    // Start is called once
    private void Start()
    {
        // Get recent camera position and rotation 
        // and save them as default ones
        _cameraDefaultPos = transform.position;
        _cameraDefaultRotation = transform.rotation;
        // Set camera pitch to zero
        _cameraPitchY = 0;
        // Subscribe to events
        _fieldControlManagerScript.FieldIsCreated -= CheckDimensionTypeOfTheField;
        _fieldControlManagerScript.FieldIsCreated += CheckDimensionTypeOfTheField;
    }

    // On disable
    private void OnDisable()
    {
        // Unsubscribe
        _fieldControlManagerScript.FieldIsCreated -= CheckDimensionTypeOfTheField;
    }

    // Update is called once per frame
    void Update()
    {
        if (_canMove == true)
        {
            // Movement
            Movement();
            // Move mouse
            UpdateMouseLook();
        }
    }

    #endregion

    #region Methods

    // Updates mouse look
    private void UpdateMouseLook()
    {
        // Creating mouse delta
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * _sensitivity;
        // Camera pitch
        _cameraPitchX += mouseDelta.x;
        _cameraPitchY -= mouseDelta.y;
        // Clamp value of camera pitch
        _cameraPitchX = Mathf.Clamp(_cameraPitchX, CAMERA_MIN_ANGLE, CAMERA_MAX_ANGLE);
        _cameraPitchY = Mathf.Clamp(_cameraPitchY, CAMERA_MIN_ANGLE, CAMERA_MAX_ANGLE);
        // Rotate
        _mainCamera.transform.localEulerAngles = Vector3.up * _cameraPitchX + Vector3.right * _cameraPitchY;
    }

    // Movement of camera
    private void Movement()
    {
        Vector3 positionBeforeMoving = transform.position;
        // Get input
        float forwardMovement = Input.GetAxis("Vertical");
        float sideMovement = Input.GetAxis("Horizontal");
        // Move our object
        transform.Translate((transform.forward * forwardMovement + transform.right * sideMovement) * _cameraMovementSpeed * Time.deltaTime);
        // If it is out of our constraints, get it back
        if ((_cameraDefaultPos - transform.position).magnitude > _cameraMovementConstraintRadius)
        {
            transform.position = positionBeforeMoving;
        }
    }

    // Updates movement ability of the camera depending on the type of the dimension of the field
    private void CheckDimensionTypeOfTheField(FieldControlManager.DimensionType dimensionType)
    {
        switch (dimensionType)
        {
            default:
                case FieldControlManager.DimensionType.Dimension2D:
                SetCameraTo2DState();
                break;
            case FieldControlManager.DimensionType.Dimension3D:
                _canMove = true;
                SetCameraTo3DState();
                break;
        }
    }

    // Sets camera to 2D state
    private void SetCameraTo2DState()
    {
        // Turn off move ability
        _canMove = false;
        // Make camera use orthographic mode
        Camera.main.orthographic = true;
        // Update position and rotation
        _mainCamera.transform.position = transform.position = _cameraDefaultPos;
        _mainCamera.transform.rotation = transform.rotation = _cameraDefaultRotation;
        // Show cursor and unlock it
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Sets camera to 3D state
    private void SetCameraTo3DState()
    {
        // Turn on move ability
        _canMove = true;
        // Make camera use perspective mode
        Camera.main.orthographic = false;
        // Hide cursor and lock it
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    #endregion
}
