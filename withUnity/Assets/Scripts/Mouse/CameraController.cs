using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    private CameraControlActions cameraActions;

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

    private float moveRadius = GameManager.mapLimit;

    //tracks where the dragging action started
    Vector3 startDrag;
    public static bool dragginTheCamera = false;

    private void Awake()
    {
        cameraActions = new CameraControlActions();
    }

    private void OnEnable()
    {
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
        DragCamera();
    }


    private void UpdateBasePosition()
    {
        speed = Mathf.Lerp(speed, maxSpeed, Time.deltaTime * acceleration);
        transform.position += speed * Time.deltaTime * targetPosition;

        //limitation of camera movement
        transform.position = Functions.KeepObjectInBounds(transform.gameObject, -moveRadius, moveRadius, -moveRadius, moveRadius);

        targetPosition = Vector3.zero;
    }

    private void ZoomCamera(InputAction.CallbackContext inputValue)
    {
        if (components.selectedObject != null)
            return;

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
        transform.position = Functions.KeepObjectInBounds(transform.gameObject, -moveRadius, moveRadius, -moveRadius, moveRadius);
        targetPosition = Vector3.zero;
    }

    private void RotateCamera(InputAction.CallbackContext inputValue)
    {
        if (!Mouse.current.middleButton.isPressed)
        {
            rotatingTheCamera = false;
            return;
        }

        if (components.selectedObject != null)
        {
            rotatingTheCamera = false;
            return;
        }

        rotatingTheCamera = true;
        float value = inputValue.ReadValue<Vector2>().x;
        transform.rotation = Quaternion.Euler(0f, value * maxRotationSpeed + transform.rotation.eulerAngles.y, 0f);
    }

    private void DragCamera()
    {
        if (!Mouse.current.rightButton.isPressed){
            dragginTheCamera = false;
            return;
        }

        if (rotatingTheCamera || components.selectedObject != null)
        {
            startDrag = Vector3.zero;
            dragginTheCamera = false;
            return;
        }

        //cant drag the camera while changing the y value of a wire
        if (MouseInteraction.changeMiddlePoint)
        {
            dragginTheCamera = false;
            return;
        }

        dragginTheCamera = true;

        Plane plane = new(Vector3.up, Vector3.zero);
        Ray ray = GameManager.cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (plane.Raycast(ray, out float distance))
        {
            if (Mouse.current.rightButton.wasPressedThisFrame || startDrag == Vector3.zero)
                startDrag = ray.GetPoint(distance);
            else
            {
                targetPosition = startDrag - ray.GetPoint(distance);
                UpdateBasePosition();
            }
        }
        return;
    }
}
