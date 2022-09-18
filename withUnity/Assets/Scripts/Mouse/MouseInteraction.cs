using UnityEngine;
using UnityEngine.InputSystem;
using static WireManager;

public class MouseInteraction : MonoBehaviour
{
    public GameObject selectedObject;
    private Vector2 offsetOnScreen;
    public static Camera cam;
    public float speed = 100;

    private Vector2 previousPosition = Vector2.zero;

    public bool changeMiddlePoint = false;
    readonly public static float changeMiddlePointSpeed = 0.05f;

    //make a reference scripts
    CameraController cameraController;

    private void Start()
    {
        cameraController = FindObjectOfType<CameraController>();
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    void Update(){
        if(cameraController.dragginTheCamera)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame && canClickThisFrame)
        {
            RaycastHit hit = CastRay();

            if (hit.collider != null)
            {
                if (!hit.collider.gameObject.CompareTag("Untagged"))
                {
                    selectedObject = hit.collider.gameObject;
                    //do stuff if clicked on a metal 
                    if (IsMetal(selectedObject))
                    {
                        //create a wire if there is no wire attached to the selectedObject
                        Wire existingWire = GetWireInMetal(selectedObject);

                        if (existingWire == null)
                        {
                            UnselectWire();
                            new Wire(selectedObject);
                            selectedObject = null;
                            return;
                        }
                        //else select the wire that already exists
                        else
                        {
                            if (existingWire != WireManager.selectedWire)
                                SelectWire(existingWire);
                            selectedObject = null;
                            return;
                        }
                    }
                    //clicked on a wire
                    else if (hit.collider.GetComponent<LineRenderer>() != null)
                    {
                        selectedObject = null;
                        Vector3 pos = hit.collider.GetComponent<LineRenderer>().GetPosition(0);
                        foreach (Wire wire in Wire._registry)
                        {
                            if (wire.lineRenderer.GetPosition(0) == pos)
                            {
                                SelectWire(wire);
                                changeMiddlePoint = true;
                                previousPosition = Mouse.current.position.ReadValue();
                                return;
                            }
                        }
                    }
                    else UnselectWire();
                }
                else UnselectWire();
            }
            else return;

            if (selectedObject != null)
            {
                Cursor.visible = false;
                offsetOnScreen = GetOffsetOfObject(selectedObject);
            }
        }
        else if (Mouse.current.middleButton.wasReleasedThisFrame)
        {
            Wire.UpdateAllMeshes();
        }

        //drag the object if there is a selected object
        if (selectedObject != null){
            if (Mouse.current.leftButton.wasReleasedThisFrame){
                selectedObject = null;
                Cursor.visible = true;
                Wire.UpdateAllMeshes();
                return;
            }

            if (MoveObjectToPosition(selectedObject)) {
                Wire.UpdateWiresPosition(selectedObject);
            }

            //rotation on right click
            if (Mouse.current.rightButton.wasPressedThisFrame){
                selectedObject.transform.rotation = Quaternion.Euler(new Vector3(
                    selectedObject.transform.rotation.eulerAngles.x,
                    selectedObject.transform.rotation.eulerAngles.y + 90f,
                    selectedObject.transform.rotation.eulerAngles.z
                ));
                Wire.UpdateWiresPosition(selectedObject);
            }
        }
        else if (WireManager.selectedWire != null)
        {
            if (changeMiddlePoint && Mouse.current.leftButton.wasReleasedThisFrame)
            {
                changeMiddlePoint = false;
                WireManager.selectedWire.UpdateMeshOfWire();
            }

            //change middle point height
            else if (changeMiddlePoint)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                if (mousePos != previousPosition)
                {
                    float delta = (mousePos.y - previousPosition.y) * changeMiddlePointSpeed;
                    previousPosition = mousePos;
                    float targetPositionY = Mathf.Min(Wire.maxMiddlePointHeight, Mathf.Max(Wire.minMiddlePointHeight, WireManager.selectedWire.middlePointHeight + delta));
                    WireManager.selectedWire.middlePointHeight = targetPositionY;
                    WireManager.selectedWire.UpdateLinesOfWire();
                }
            }
        }

    }

    public bool MoveObjectToPosition(GameObject obj)
    {
        //avoid unnecessary calculations if the mouse positions remains the same
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        if (mousePosition == previousPosition) return false;

        previousPosition = mousePosition;
        Vector3 targetPosition = GetNewPosition(mousePosition, offsetOnScreen, obj.transform.position);
        //move the object to the target position smoothly
        obj.transform.position = Vector3.Lerp(obj.transform.position, targetPosition, speed * Time.deltaTime);
        return true;
    }

    public static Vector3 GetOffsetOfObject(GameObject obj)
    {
        //calculate the offset of the position where the mouse is clicked and 
        //the actual screen position of the object
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 screenPoint = cam.WorldToScreenPoint(obj.transform.position);
        return mousePosition - screenPoint;
    }

    public static Vector3 GetNewPosition(Vector2 mousePosition, Vector2 offset, Vector3 toUpdatePosition){
        Vector3 screenPoint = cam.WorldToScreenPoint(toUpdatePosition);

        //calculate the target world position based on the mouse input
        Vector3 screenPosition = new Vector3(mousePosition.x- offset.x, mousePosition.y-offset.y, screenPoint.z);
        Vector3 worldPosition = cam.ScreenToWorldPoint(screenPosition);
        
        Vector3 targetPosition = new Vector3(worldPosition.x, toUpdatePosition.y, worldPosition.z);
        return targetPosition;
    }
    
    public static RaycastHit CastRay(){
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
