using UnityEngine;
using UnityEngine.InputSystem;
using static WireManager;

public class MouseInteraction : MonoBehaviour
{
    public GameObject selectedObject;
    private Vector2 offsetOnScreen;
    public readonly float speed = 100;

    private Vector2 previousPosition = Vector2.zero;

    public bool changeMiddlePoint = false;
    public readonly static float changeMiddlePointSpeed = 0.05f;

    void Update(){

        //if(CameraController.dragginTheCamera && Wire.justCreated == null)
        //    return;

        if (Wire.justCreated != null)
        {
            Wire.justCreated.WireFollowMouse(Wire.justCreated);
            if (LED.justCreated != null)
            {
                //update the led position so that its always between the start and mouse point
                LED.justCreated.Move();
                LED.justCreated.wire2.UpdateLinesOfWire();
                LED.justCreated.wire1.UpdateLinesOfWire();
            }
        }
        else if (selectedObject != null)
        {
            Cursor.visible = false;
            if (MoveObjectToPosition(selectedObject))
            {
                Wire.UpdateWiresPosition(selectedObject);
                LED.UpdatePositionsAndRotations(selectedObject);
            }
        }
        else if (selectedWire != null)
        {
            if (changeMiddlePoint)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                if (mousePos != previousPosition)
                {
                    float delta = (mousePos.y - previousPosition.y) * changeMiddlePointSpeed;
                    previousPosition = mousePos;
                    float targetPositionY = Mathf.Min(Wire.maxMiddlePointHeight, Mathf.Max(Wire.minMiddlePointHeight, WireManager.selectedWire.middlePointHeight + delta));
                    selectedWire.middlePointHeight = targetPositionY;
                    selectedWire.UpdateLinesOfWire();
                }
            }
        }
        else if (ItemManager.selectedItem != null)
        {
            
        }

        //------------LEFT BUTTON PRESSED-------------
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            RaycastHit hit = CastRay();
            if (hit.collider != null)
            {
                selectedObject = hit.collider.gameObject; 
                offsetOnScreen = GetOffsetOfObject(selectedObject);

                //clicked on plane for example
                if (selectedObject.CompareTag("Untagged"))
                {
                    selectedObject = null;
                }

                //clicked on a metal
                else if (IsMetal(selectedObject))
                {
                    Wire existingWire = GetWireInMetal(selectedObject);

                    //if there is already a wire, select that wire and dont create a new one
                    if (existingWire != null)
                    {
                        SelectWire(existingWire);
                        selectedObject = null;
                    }

                    //WIRE
                    else if (GameManager.tabItem == 0)
                    {
                        //start creating a new wire
                        if (Wire.justCreated == null)
                        {
                            //create a wire 
                            UnselectWire();
                            new Wire(selectedObject);
                            selectedObject = null;
                            return;
                        }
                        //finish creation of a new wire
                        else if (hit.collider.gameObject != Wire.justCreated.startObject)
                        {
                            Wire.justCreated.endObject = hit.collider.gameObject; 
                            Wire.justCreated.lineRenderer.SetPosition(Wire.justCreated.verticesAmount - 1, hit.collider.gameObject.transform.position);

                            Wire.justCreated.FinishWireCreation();

                            //Update the electricity parameters of all wires
                            UpdateElectricityParameters();
                            Wire.justCreated = null;
                            selectedObject = null;
                            return;
                        }
                    }

                    //LED
                    else if (GameManager.tabItem == 1)
                    {
                        if (LED.justCreated == null)
                        {
                            //create an led
                            UnselectWire();
                            new LED(selectedObject);
                            selectedObject = null;
                            return;
                        }
                        else if (hit.collider.gameObject != LED.justCreated.wire2.startObject && hit.collider.gameObject != LED.justCreated.wire1.endObject)
                        {
                            //endobject of LED item
                            LED.justCreated.endObject = hit.collider.gameObject;

                            LED.justCreated.wire2.endObject = hit.collider.gameObject;
                            LED.justCreated.wire2.lineRenderer.SetPosition(LED.justCreated.wire2.verticesAmount - 1, hit.collider.gameObject.transform.position);

                            LED.justCreated.wire1.FinishWireCreation();
                            LED.justCreated.wire2.FinishWireCreation();

                            //Update the electricity parameters of all wires
                            UpdateElectricityParameters();

                            LED.justCreated = null;
                            Wire.justCreated = null;
                            selectedObject = null;
                            return;
                        }
                    }
                }
                //clicked on a wire
                else if (IsWire(hit.collider))
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
                //clicked on an item
                else if (IsItem(selectedObject))
                {
                    ItemManager.selectedItem = selectedObject;
                    selectedObject = null;
                }
                else
                {
                    UnselectWire();
                } 
            }
        }

        //-----------LEFT BUTTON RELEASED-------------
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (selectedObject != null)
            {
                selectedObject = null;
                Cursor.visible = true;
                Wire.UpdateAllMeshes();
                return;
            }
            else if (selectedWire != null)
            {
                if (changeMiddlePoint )
                changeMiddlePoint = false;
                selectedWire.UpdateMeshOfWire();
            }
        }

        //-----------RIGHT BUTTON PRESSED-------------
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (LED.justCreated != null)
            {
                Destroy(LED.justCreated.LEDObject);
                Destroy(LED.justCreated.wire1.lineObject);
                Destroy(LED.justCreated.wire2.lineObject);
                LED.justCreated = null;
                Wire.justCreated = null;
                return;
            }
            if (Wire.justCreated != null) {
                //cancel creation of a new wire
                ResetWire();
                Wire.justCreated = null;
                return;
            }
            else if (selectedObject != null){
                //rotation on right click
                selectedObject.transform.rotation = Quaternion.Euler(new Vector3(
                    selectedObject.transform.rotation.eulerAngles.x,
                    selectedObject.transform.rotation.eulerAngles.y + 90f,
                    selectedObject.transform.rotation.eulerAngles.z
                ));
                Wire.UpdateWiresPosition(selectedObject);
            }
        }

        //----------MIDDLE BUTTON RELEASED-------------
        else if (Mouse.current.middleButton.wasReleasedThisFrame)
        {
            Wire.UpdateAllMeshes();
        }

        //----------KEYBOARD BUTTONS PRESSED------------
        //delete wire when pressing delete-key
        if (Keyboard.current.deleteKey.wasPressedThisFrame)
        {
            if (selectedWire != null)
            {
                //remove wire from attachedWires List of start/end object
                selectedWire.startObject.transform.parent.gameObject.GetComponent<Properties>().attachedWires.Remove(selectedWire);
                selectedWire.endObject.transform.parent.gameObject.GetComponent<Properties>().attachedWires.Remove(selectedWire);

                Wire._registry.Remove(selectedWire);
                Destroy(selectedWire.lineObject);
                selectedWire = null;

                //Update the electricity parameters of all wires
                UpdateElectricityParameters();
            }
        }

        //make the wire flat if the F-key pressed
        else if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (selectedWire != null)
            {
                if (!selectedWire.flat)
                {
                    //make wire flat
                    selectedWire.flat = true;
                    selectedWire.MakeWireFlat();
                }
                else
                {
                    //back to curve
                    selectedWire.flat = false;
                    selectedWire.MakeWireCurve();
                }
            }
        }

        //go through list of possible items when pressing the tab-key
        else if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            if (Wire.justCreated == null)
            {
                GameManager.tabItem += 1;
                //reset if too high (for now there are only 2 items -> wire, led)
                if (GameManager.tabItem > 1)
                    GameManager.tabItem = 0;
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
        Vector2 screenPoint = GameManager.cam.WorldToScreenPoint(obj.transform.position);
        return mousePosition - screenPoint;
    }

    public static Vector3 GetNewPosition(Vector2 mousePosition, Vector2 offset, Vector3 toUpdatePosition){
        Vector3 screenPoint = GameManager.cam.WorldToScreenPoint(toUpdatePosition);

        //calculate the target world position based on the mouse input
        Vector3 screenPosition = new Vector3(mousePosition.x- offset.x, mousePosition.y-offset.y, screenPoint.z);
        Vector3 worldPosition = GameManager.cam.ScreenToWorldPoint(screenPosition);
        
        Vector3 targetPosition = new Vector3(worldPosition.x, toUpdatePosition.y, worldPosition.z);
        return targetPosition;
    }
    
    public static RaycastHit CastRay(){
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 screenMousePosFar = new Vector3(
            mousePosition.x,
            mousePosition.y,
            GameManager.cam.farClipPlane
        );

        Vector3 screenMousePosNear = new Vector3(
            mousePosition.x,
            mousePosition.y,
            GameManager.cam.nearClipPlane
        );

        Vector3 worldMousePosFar = GameManager.cam.ScreenToWorldPoint(screenMousePosFar);
        Vector3 worldMousePosNear = GameManager.cam.ScreenToWorldPoint(screenMousePosNear);

        Physics.Raycast(worldMousePosNear, worldMousePosFar - worldMousePosNear, out RaycastHit hit);

        return hit;
    }

}
