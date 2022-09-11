using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;
using static WireManager;

public class MouseInteraction : MonoBehaviour
{
    public GameObject selectedObject;
    private Vector2 offsetOnScreen;
    [SerializeField] private Camera cam;
    public float speed = 100;

    private Vector2 previousPosition = Vector2.zero;

    //make a reference scripts
    CameraController cameraController;

    private void Start()
    {
        cameraController = FindObjectOfType<CameraController>();
    }

    void Update(){
        if(cameraController.dragginTheCamera)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (selectedObject == null)
            {
                RaycastHit hit = CastRay();

                if (hit.collider != null)
                {
                    if (hit.collider.gameObject.tag != "Untagged")
                    {
                        selectedObject = hit.collider.gameObject;

                        //do stuff if clicked on a metal 
                        if (selectedObject.transform.parent != null)
                        {
                            if (selectedObject.transform.parent.tag == "Metals")
                            {
                                new Wire(selectedObject, selectedObject.transform.parent.gameObject.transform.parent.tag);
                                selectedObject = null;
                                return;
                            }
                        }
                    }
                }
                else return;

                if (selectedObject != null)
                {
                    Cursor.visible = false;

                    //calculate the offset of the position where the mouse is clicked and 
                    //the actual screen position of the object
                    Vector2 mousePosition = Mouse.current.position.ReadValue();
                    Vector2 screenPoint = cam.WorldToScreenPoint(selectedObject.transform.position);
                    offsetOnScreen = mousePosition - screenPoint;
                }
            }
        }

        //drag the object if there is a selected object
        if (selectedObject != null){
            if (Mouse.current.leftButton.wasReleasedThisFrame){
                selectedObject = null;
                Cursor.visible = true;
                return;
            }

            //avoid unnecessary calculations if the mouse positions stays the same
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            if (mousePosition != previousPosition)
            {
                previousPosition = mousePosition;
                setNewPosition(mousePosition);
                updateWiresPosition();
            }
            
            //rotation on right click
            if (Mouse.current.rightButton.wasPressedThisFrame){
                selectedObject.transform.rotation = Quaternion.Euler(new Vector3(
                    selectedObject.transform.rotation.eulerAngles.x,
                    selectedObject.transform.rotation.eulerAngles.y + 90f,
                    selectedObject.transform.rotation.eulerAngles.z
                ));
            }
        }
    }

    void updateWiresPosition()
    {
        Debug.Log("new pos");
    }

    void setNewPosition(Vector2 mousePosition){
        Vector3 screenPoint = cam.WorldToScreenPoint(selectedObject.transform.position);

        //calculate the target world position based on the mouse input
        Vector3 screenPosition = new Vector3(mousePosition.x-offsetOnScreen.x, mousePosition.y-offsetOnScreen.y, screenPoint.z);
        Vector3 worldPosition = cam.ScreenToWorldPoint(screenPosition);
        Vector3 targetPosition = new Vector3(worldPosition.x, selectedObject.transform.position.y, worldPosition.z);

        //move the object to the target position smoothly
        selectedObject.transform.position = Vector3.Lerp(selectedObject.transform.position, targetPosition, speed * Time.deltaTime);
    }
    
    private RaycastHit CastRay(){
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 screenMousePosFar = new Vector3(
            mousePosition.x,
            mousePosition.y,
            cam.farClipPlane
        );

        Vector3 screenMousePosNear = new Vector3(
            mousePosition.x,
            mousePosition.y,
            cam.nearClipPlane
        );

        Vector3 worldMousePosFar = cam.ScreenToWorldPoint(screenMousePosFar);
        Vector3 worldMousePosNear = cam.ScreenToWorldPoint(screenMousePosNear);

        RaycastHit hit;
        Physics.Raycast(worldMousePosNear, worldMousePosFar - worldMousePosNear, out hit);

        return hit;
    }

}
