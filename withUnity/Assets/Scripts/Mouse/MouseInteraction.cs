using System.Collections.Generic;
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
    public readonly static float changeItemYSpeed = 0.01f;

    void Update(){

        //if(CameraController.dragginTheCamera && Wire.justCreated == null)
        //    return;

        if (Wire.justCreated != null)
        {
            Wire.justCreated.WireFollowMouse(Wire.justCreated);
            if (Item.justCreated != null)
            {
                //update the items position so that its always between the start and mouse point
                Item.justCreated.UpdateItem();
            }
        }
        else if (selectedObject != null)
        {
            Cursor.visible = false;
            if (MoveObjectToPosition(selectedObject))
            {
                Wire.UpdateWirePositionComponent(selectedObject);
                Item.UpdateItemAll(selectedObject);
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
                    selectedWire.UpdatePointsOfWire();
                }
            }
        }
        else if (Item.selectedItem != null)
        {
            if (Item.moveItemUpDown)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                if (mousePos != previousPosition)
                {
                    float delta = (mousePos.y - previousPosition.y) * changeItemYSpeed;
                    previousPosition = mousePos;

                    //ITEM
                    float targetPositionY = Mathf.Min(Item.maxItemY, Mathf.Max(Item.minItemY, Item.selectedItem.itemObject.transform.position.y + delta));
                    Vector3 pos = Item.selectedItem.itemObject.transform.position;
                    Item.selectedItem.UpdateYPosition(new Vector3(pos.x, targetPositionY, pos.z));
                }
            }
        }

        //------------LEFT BUTTON PRESSED-------------
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            RaycastHit hit = CastRay();
            if (hit.collider != null)
            {
                selectedObject = hit.collider.gameObject; 
                offsetOnScreen = GetOffsetOfObject(selectedObject);

                UnselectWire();
                Item.Unselect();

                //clicked on plane for example
                if (selectedObject.CompareTag("Untagged"))
                    selectedObject = null;

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

                    //ITEM
                    else if (GameManager.tabItem > 0)
                    {
                        if (Item.justCreated == null)
                        {
                            if (GameManager.tabItem == 1)
                                new Item(selectedObject, "LED");
                            else if (GameManager.tabItem == 2)
                                new Item(selectedObject, "otheritem");

                            //create an item
                            selectedObject = null;
                            return;
                        }
                        else if (hit.collider.gameObject != Item.justCreated.wire2.startObject && hit.collider.gameObject != Item.justCreated.wire1.endObject)
                        {
                            //endobject of LED item
                            Item.justCreated.endObject = hit.collider.gameObject;

                            Item.justCreated.wire2.endObject = hit.collider.gameObject;
                            
                            Item.justCreated.wire1.FinishWireCreation();
                            Item.justCreated.wire2.FinishWireCreation();

                            Item.justCreated.UpdateItem();

                            //Update the electricity parameters of all wires
                            UpdateElectricityParameters();

                            Item.justCreated = null;
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
                    foreach (Item item in Item._registry)
                    {
                        if (selectedObject == item.itemObject)
                        {
                            Item.selectedItem = item;
                            Item.moveItemUpDown = true;
                            previousPosition = Mouse.current.position.ReadValue();
                            break;
                        }
                    }
                    selectedObject = null;
                }
            }
        }

        //-----------LEFT BUTTON RELEASED-------------
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (Item.selectedItem != null)
            {
                Item.selectedItem.wire1.UpdateMeshOfWire();
                Item.selectedItem.wire2.UpdateMeshOfWire();
                Item.moveItemUpDown = false;
            }

            if (selectedObject != null)
            {
                selectedObject = null;
                Cursor.visible = true;
                Wire.UpdateAllMeshes();
                return;
            }
            else if (selectedWire != null)
            {
                if (changeMiddlePoint)
                    changeMiddlePoint = false;
                selectedWire.UpdateMeshOfWire();
            }
        }

        //-----------RIGHT BUTTON PRESSED-------------
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (Item.justCreated != null)
            {
                Destroy(Item.justCreated.itemObject);
                Destroy(Item.justCreated.wire1.lineObject);
                Destroy(Item.justCreated.wire2.lineObject);
                Item.justCreated = null;
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
                Wire.UpdateWirePositionComponent(selectedObject);
                Item.UpdateItemAll(selectedObject);
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
            WireManager.ResetMaterialHighlight();
            List<Wire> wiresToDelete = new List<Wire>();
            bool deleteWires = false;

            //delete a selected item
            if (Item.selectedItem != null)
            {
                wiresToDelete = Item.selectedItem.wiresOfItem;
                Item._registry.Remove(Item.selectedItem);
                Destroy(Item.selectedItem.itemObject);
                Item.Unselect();
                deleteWires = true;
            }
            else if (selectedWire != null)
            {
                //delete item that wire is attached to
                if (IsAttachedToItem(selectedWire))
                {
                    //find the item object
                    foreach (Item item in Item._registry)
                    {
                        if (item.wire1 == selectedWire || item.wire2 == selectedWire)
                        {
                            wiresToDelete = item.wiresOfItem;
                            Item._registry.Remove(item);
                            Destroy(item.itemObject);
                            break;
                        }
                    }
                }
                //else just delete the selected wire
                else 
                    wiresToDelete.Add(selectedWire);
                deleteWires = true;
            }
            foreach (Wire wire in wiresToDelete)
            {
                //remove wire from attachedWires List of start/end object
                wire.startObject.transform.parent.gameObject.GetComponent<Properties>().attachedWires.Remove(wire);
                wire.endObject.transform.parent.gameObject.GetComponent<Properties>().attachedWires.Remove(wire);
                Wire._registry.Remove(wire);
                Destroy(wire.lineObject);
            }

            if (deleteWires)
            {
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
