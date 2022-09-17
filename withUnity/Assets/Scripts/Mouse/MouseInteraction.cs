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
                    UnselectWire();
                    if (!hit.collider.gameObject.CompareTag("Untagged"))
                    {
                        selectedObject = hit.collider.gameObject;

                        //do stuff if clicked on a metal 
                        if (IsMetal(selectedObject))
                        {
                            //create a wire if there is no wire attached to the selectedObject
                            Wire existingWire = WireAlreadyExists(selectedObject);

                            //print the current of the metal
                            /*if (selectedObject.CompareTag("BatteryMetal")) {
                                Debug.Log(selectedObject.GetComponent<Properties>().current);
                            }
                            else {
                                Debug.Log(selectedObject.transform.parent.gameObject.GetComponent<Properties>().current);
                            }*/

                            if (existingWire == null)
                            {
                                new Wire(selectedObject);
                                selectedObject = null;
                                return;
                            }
                            //else select the wire that already exists
                            else
                            {
                                SelectWire(existingWire);
                            }
                        }

                        //NO BOX COLLIDER ??
                        else if (selectedObject.CompareTag("Wire"))
                        {
                            Debug.Log(selectedObject);
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

            //avoid unnecessary calculations if the mouse positions remains the same
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            if (mousePosition != previousPosition)
            {
                previousPosition = mousePosition;
                Vector3 targetPosition = GetNewPosition(cam, mousePosition, offsetOnScreen, selectedObject.transform.position);
                //move the object to the target position smoothly
                selectedObject.transform.position = Vector3.Lerp(selectedObject.transform.position, targetPosition, speed * Time.deltaTime);
                UpdateWiresPosition();
            }
            
            //rotation on right click
            if (Mouse.current.rightButton.wasPressedThisFrame){
                selectedObject.transform.rotation = Quaternion.Euler(new Vector3(
                    selectedObject.transform.rotation.eulerAngles.x,
                    selectedObject.transform.rotation.eulerAngles.y + 90f,
                    selectedObject.transform.rotation.eulerAngles.z
                ));
                UpdateWiresPosition();
            }
        }
    }

    void UpdateWiresPosition()
    {
        //this method updates the wire positions when a component is being moved
        foreach (Wire wire in Wire._registry)
        {
            bool updateVertices = false;
            if (wire.startObject.transform.IsChildOf(selectedObject.transform)) {
                wire.lineRenderer.SetPosition(0, wire.startObject.transform.position);
                updateVertices = true;
            }
            if (wire.endObject.transform.IsChildOf(selectedObject.transform)) {
                wire.lineRenderer.SetPosition(wire.verticesAmount - 1, wire.endObject.transform.position);
                updateVertices = true;
            }
            if (updateVertices)
                wire.UpdateLinesOfWire();
        }
    }

    public static Vector3 GetNewPosition(Camera camera, Vector2 mousePosition, Vector2 offset, Vector3 toUpdatePosition){
        Vector3 screenPoint = camera.WorldToScreenPoint(toUpdatePosition);

        //calculate the target world position based on the mouse input
        Vector3 screenPosition = new Vector3(mousePosition.x- offset.x, mousePosition.y-offset.y, screenPoint.z);
        Vector3 worldPosition = camera.ScreenToWorldPoint(screenPosition);
        
        Vector3 targetPosition = new Vector3(worldPosition.x, toUpdatePosition.y, worldPosition.z);
        return targetPosition;
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

        Physics.Raycast(worldMousePosNear, worldMousePosFar - worldMousePosNear, out RaycastHit hit);

        return hit;
    }

}
