using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Progress;
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
        else if (Selection.oneWireIsSelected)
        {
            if (changeMiddlePoint)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                if (mousePos != previousPosition)
                {
                    float delta = (mousePos.y - previousPosition.y) * changeMiddlePointSpeed;
                    previousPosition = mousePos;
                    
                    //update the middle point height of all selected wires
                    foreach (Selection currentlySelected in Selection.currentlySelectedWires)
                    {
                        float targetPositionY = Mathf.Min(Wire.maxMiddlePointHeight, Mathf.Max(Wire.minMiddlePointHeight, currentlySelected.wire.middlePointHeight + delta));
                        currentlySelected.wire.middlePointHeight = targetPositionY;
                        currentlySelected.wire.UpdatePointsOfWire();
                    }
                    
                }
            }
        }
        else if (Selection.oneItemIsSelected)
        {
            if (Item.moveItemUpDown)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                if (mousePos != previousPosition)
                {
                    float delta = (mousePos.y - previousPosition.y) * changeItemYSpeed;
                    previousPosition = mousePos;

                    //update the y position of all selected items
                    foreach (Selection currentlySelected in Selection.currentlySelectedItems)
                    {
                        float targetPositionY = Mathf.Min(Item.maxItemY, Mathf.Max(Item.minItemY, currentlySelected.item.itemObject.transform.position.y + delta));
                        Vector3 pos = currentlySelected.item.itemObject.transform.position;
                        currentlySelected.item.UpdateYPosition(new Vector3(pos.x, targetPositionY, pos.z));
                    }
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

                //unselect wires only if multi selection is disabled
                if (!Selection.multiSelectionEnabled)
                {
                    Selection.UnselectSelection();
                }

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
                        new Selection(existingWire);
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
                                new Item(selectedObject, "LED", "blue");
                            else if (GameManager.tabItem == 2)
                                new Item(selectedObject, "Resistor");

                            //create an item
                            selectedObject = null;
                            return;
                        }
                        else if (hit.collider.gameObject != Item.justCreated.startObject)
                        {
                            //endobject of LED item
                            Item.justCreated.endObject = hit.collider.gameObject;

                            Item.justCreated.wire2.endObject = hit.collider.gameObject;
                            
                            Item.justCreated.wire1.FinishWireCreation();
                            Item.justCreated.wire2.FinishWireCreation();

                            Item._registry.Add(Item.justCreated);

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
                    Wire wire = hit.collider.gameObject.GetComponent<Properties>().wire;

                    //select only one wire if its only a wire (no item)
                    if (!IsAttachedToItem(wire) 
                        || wire.startObject.name == "mn" 
                        || wire.startObject.name == "mp"
                        || wire.endObject.name == "mn"
                        || wire.endObject.name == "mp")
                    {
                        changeMiddlePoint = true;
                        new Selection(wire);
                    }

                    //select the item and all wires attached to the item
                    else
                    {
                        Item item = GetItemAttachedToWire(wire);
                        new Selection(null, item);
                        foreach (Wire itemWire in item.itemObject.GetComponent<Properties>().attachedWires)
                            new Selection(itemWire);
                    }
                    previousPosition = Mouse.current.position.ReadValue();
                    return;
                }
                //clicked on an item (and not on battery)
                else if (IsItem(selectedObject) && selectedObject.GetComponent<Properties>().battery == null)
                {
                    //select the wire and all wires attached to the item
                    Item item = selectedObject.GetComponent<Properties>().item;
                    new Selection(null, item);
                    foreach (Wire itemWire in item.itemObject.GetComponent<Properties>().attachedWires)
                        new Selection(itemWire);

                    Item.moveItemUpDown = true;
                    previousPosition = Mouse.current.position.ReadValue();
                    selectedObject = null;
                }
            }
        }

        //-----------LEFT BUTTON RELEASED-------------
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (Selection.oneItemIsSelected)
            {
                foreach (Selection current in Selection.currentlySelectedItems)
                {
                    current.item.wire1.UpdateMeshOfWire();
                    current.item.wire2.UpdateMeshOfWire();
                }
                Item.moveItemUpDown = false;
            }

            if (selectedObject != null)
            {
                selectedObject = null;
                Cursor.visible = true;
                Wire.UpdateAllMeshes();
                return;
            }
            else if (Selection.oneWireIsSelected)
            {
                if (changeMiddlePoint)
                    changeMiddlePoint = false;
                
                //update mesh for all selected wires
                foreach (Selection currentlySelected in Selection.currentlySelectedWires)
                    currentlySelected.wire.UpdateMeshOfWire();
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
        //delete all selected things when pressing delete-key
        if (Keyboard.current.deleteKey.wasPressedThisFrame)
        {
            if (Selection.oneItemIsSelected || Selection.oneWireIsSelected)
            {
                Selection.DeleteSelection();
                Selection.UnselectSelection();
                UpdateElectricityParameters();
            }
        }

        //make the wire flat if the F-key pressed
        else if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (Selection.oneWireIsSelected)
            {
                //toggle between flat and curve for all selected wires
                foreach (Selection currentlySelected in Selection.currentlySelectedWires)
                {
                    if (!currentlySelected.wire.flat)
                    {
                        //make wire flat
                        currentlySelected.wire.flat = true;
                        currentlySelected.wire.MakeWireFlat();
                    }
                    else
                    {
                        //back to curve
                        currentlySelected.wire.flat = false;
                        currentlySelected.wire.MakeWireCurve();
                    }
                }
            }
        }

        //go through list of possible items when pressing the tab-key
        else if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            if (Wire.justCreated == null)
            {
                GameManager.tabItem += 1;
                //reset to first item 
                if (GameManager.tabItem > ItemManager.itemNumber-1)
                    GameManager.tabItem = 0;
            }
        }

        //space bar to toggle electricity path view
        else if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            electricityPathView = !electricityPathView;
            foreach (Wire wire in connectedWires)
            {
                if (electricityPathView)
                    wire.lineRenderer.material = ResourcesManager.yellow;
                else
                    wire.lineRenderer.material = wire.wireColor;
            }
            Node.SetNeighborNodes();
            //Node.PrintNodes();
            Node.PrintNeighbors();
            //Node.PrintNeighborResistors();
        }

        //ctrl-key for enabling multiselection
        else if (Keyboard.current.ctrlKey.wasPressedThisFrame)
        {
            Selection.multiSelectionEnabled = true;
        }
        else if (Keyboard.current.ctrlKey.wasReleasedThisFrame)
        {
            Selection.multiSelectionEnabled = false;
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
