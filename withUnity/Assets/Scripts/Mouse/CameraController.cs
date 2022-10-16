using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

public class CameraController : MonoBehaviour
{
    private CameraControlActions cameraActions;
    private InputAction movement;
    private Transform cameraTransform;

    //make a reference to the MouseInteraction script
    MouseInteraction components;

    //horizontal motion
    [SerializeField]
    private float maxSpeed = 5f;
    private float speed;
    [SerializeField]
    private float acceleration = 10f;

    //vertical motion - zooming
    [SerializeField]
    private float minHeight = 1f;
    [SerializeField]
    private float maxHeight = 15f;
    [SerializeField]
    private float zoomFactor = 5f;

    //rotation
    [SerializeField]
    private float maxRotationSpeed = 1f;
    private static bool rotatingTheCamera = false;

    //value set in various functions
    //used to update the position of the camera base object
    private Vector3 targetPosition;

    //used to track and maintain velocity w/o a rigidbody
    private Vector3 lastPosition;
    private float moveRadius = 12f;

    //tracks where the dragging action started
    Vector3 startDrag;
    public static bool dragginTheCamera = false;

    private void Awake()
    {
        cameraActions = new CameraControlActions();
        cameraTransform = GetComponentInChildren<Camera>().transform;
    }

    private void OnEnable()
    {
        //cameraTransform.LookAt(this.transform);

        lastPosition = transform.position;
        movement = cameraActions.Camera.Movement;
        cameraActions.Camera.RotateCamera.performed += RotateCamera;
        cameraActions.Camera.ZoomCamera.performed += ZoomCamera;
        cameraActions.Camera.Enable();
    }

    private void OnDisable()
    {
        cameraActions.Camera.RotateCamera.performed -= RotateCamera;
        cameraActions.Camera.ZoomCamera.performed -= ZoomCamera;
        cameraActions.Disable();
    }

    private void Start()
    {
        components = FindObjectOfType<MouseInteraction>();
    }

    private void Update()
    {
        if (components.selectedObject != null)
            return;
        
        if (!rotatingTheCamera && (DragCamera() || MoveWithKeyboard()))
        {
            UpdateBasePosition();
        }
    }


    private void UpdateBasePosition()
    {
        lastPosition = transform.position;
        speed = Mathf.Lerp(speed, maxSpeed, Time.deltaTime * acceleration);
        transform.position += speed * Time.deltaTime * targetPosition;

        //limitation of camera movement
        LimitCameraMovement();

        targetPosition = Vector3.zero;
    }

    private bool MoveWithKeyboard()
    {
        Vector3 inputValue = movement.ReadValue<Vector2>().x * GetCameraRight()
                               + movement.ReadValue<Vector2>().y * GetCameraForward();

        if (inputValue == Vector3.zero)
            return false;

        inputValue = inputValue.normalized;
        if(inputValue.sqrMagnitude > 0.1f)
            targetPosition += inputValue;

        return true;
    }

    private Vector3 GetCameraRight()
    {
        Vector3 right = cameraTransform.right;
        right.y = 0;
        return right;
    }

    private Vector3 GetCameraForward()
    {
        Vector3 forward = cameraTransform.forward;
        forward.y = 0;
        return forward;
    }

    private void ZoomCamera(InputAction.CallbackContext inputValue)
    {
        float value = inputValue.ReadValue<Vector2>().y;

        Camera cam = GameManager.cam;
        Vector2 pos = Mouse.current.position.ReadValue();

        // Get Position before and after zooming
        Vector3 mouseOnWorld = cam.ScreenToWorldPoint(pos);
        cam.orthographicSize = Mathf.Clamp(GameManager.cam.orthographicSize - value * zoomFactor / 100, minHeight, maxHeight);

        // Calculate Difference between Positions before and after Zooming
        Vector3 posDiff = mouseOnWorld - cam.ScreenToWorldPoint(pos);

        // Apply Target-Position to Camera
        transform.position += posDiff;
        LimitCameraMovement();
    }

    private void RotateCamera(InputAction.CallbackContext inputValue)
    {
        if (!Mouse.current.middleButton.isPressed || dragginTheCamera)
        {
            rotatingTheCamera = false;
            return;
        }

        rotatingTheCamera = true;
        float value = inputValue.ReadValue<Vector2>().x;
        transform.rotation = Quaternion.Euler(0f, value * maxRotationSpeed + transform.rotation.eulerAngles.y, 0f);

    }

    private bool DragCamera()
    {
        if (!Mouse.current.rightButton.isPressed){
            dragginTheCamera = false;
            return false;
        }

        //cant drag the camera while changing the y value of a wire
        if (MouseInteraction.changeMiddlePoint)
            return false;

        dragginTheCamera = true;

        Plane plane = new(Vector3.up, Vector3.zero);
        Ray ray = GameManager.cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if(plane.Raycast(ray, out float distance))
        {
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                targetPosition = Vector3.zero;
                startDrag = ray.GetPoint(distance);
            }
            else
                targetPosition += startDrag - ray.GetPoint(distance);
        }
        return true;
    }

    private void LimitCameraMovement()
    {
        if (transform.position.x > moveRadius)
            transform.position = new Vector3(moveRadius, lastPosition.y, transform.position.z);
        else if (transform.position.x < -moveRadius)
            transform.position = new Vector3(-moveRadius, lastPosition.y, transform.position.z);
        if (transform.position.z > moveRadius)
            transform.position = new Vector3(transform.position.x, lastPosition.y, moveRadius);
        else if (transform.position.z < -moveRadius)
            transform.position = new Vector3(transform.position.x, lastPosition.y, -moveRadius);
    }
}
