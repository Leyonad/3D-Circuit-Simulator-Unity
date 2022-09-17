using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

public class WireManager : MonoBehaviour
{
    [SerializeField]
    GameObject debugObject;
    [SerializeField] 
    public static Camera cam;
    public int numCapVertices = 4;

    public static Wire selectedWire;

    private void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    private void Update()
    {
        if (Wire.justCreated != null)
        {
            Wire.justCreated.WireFollowMouse(Wire.justCreated);

            //cancel creation of a new wire by destroying it
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                Destroy(Wire.justCreated.lineObject);
                Wire.justCreated = null;
            }
            //finish creation of a new wire by adding it to _registry
            else if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                //check if clicked on a gameobject
                RaycastHit hit = CastRay();

                bool wirePossible = false;
                if (hit.collider != null)
                    if (IsMetal(hit.collider.gameObject))
                        wirePossible = true;
                
                if (wirePossible)
                {
                    //Check if the wire doesnt already exist
                    Wire.justCreated.endObject = hit.collider.gameObject;
                    if (Wire.justCreated.endObject != Wire.justCreated.startObject && WireAlreadyExists(Wire.justCreated.endObject) == null)
                    {
                        //attach wire to attachedWires List of start/end object
                        Wire.justCreated.startObject.transform.parent.gameObject.GetComponent<Properties>().attachedWires.Add(Wire.justCreated);
                        Wire.justCreated.endObject.transform.parent.gameObject.GetComponent<Properties>().attachedWires.Add(Wire.justCreated);

                        Wire.justCreated.lineRenderer.SetPosition(Wire.justCreated.verticesAmount - 1, hit.collider.gameObject.transform.position);
                        Wire.justCreated.UpdateLinesOfWire();
                        Wire._registry.Add(Wire.justCreated);

                        //Update the electricity parameters of all wires
                        UpdateElectricityParameters();
                    }
                    else Destroy(Wire.justCreated.lineObject);
                }
                else Destroy(Wire.justCreated.lineObject);
                
                //always set to null if mouse pressed
                Wire.justCreated = null;
            }
        }

        //delete wire when pressing delete-key
        else if (Keyboard.current.deleteKey.wasReleasedThisFrame)
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
    }

    public static void SelectWire(Wire wire)
    {
        selectedWire = wire;
        selectedWire.lineRenderer.material = ResourcesManager.highlightWireMaterial;
    }

    public static void UnselectWire()
    {
        if (selectedWire == null)
            return;
        selectedWire.lineRenderer.material = ResourcesManager.wireMaterial;
        selectedWire = null;
    }

    public static bool IsMetal(GameObject obj)
    {
        if (obj.CompareTag("Metal"))
            return true;
        return false;
    }

    public static Wire WireAlreadyExists(GameObject wireToObject)
    {
        foreach (Wire existingWire in Wire._registry)
        {
            if (wireToObject == existingWire.startObject
                || wireToObject == existingWire.endObject
                || wireToObject == existingWire.startObject
                || wireToObject == existingWire.endObject)
            {
                //Debug.Log("WIRE ALREADY EXISTS!");
                return existingWire;
            }
        }
        return null;
    }

    private void UpdateElectricityParameters()
    {
        return;
        GameObject startParent = null;
        Wire startWire = null;
        //reset all metalstrips and battery metals, except metal2 of the battery
        foreach (Wire wire in Wire._registry)
        {
            if (wire.startObject.transform.parent.name != "Metal2") {
                wire.startObject.transform.parent.GetComponent<Properties>().current = 0;
            }
            else {
                startParent = wire.startObject.transform.parent.gameObject;
                startWire = wire;
            }
            if (wire.endObject.transform.parent.name != "Metal2")  {
                wire.endObject.transform.parent.GetComponent<Properties>().current = 0;
            }
            else {
                startParent = wire.endObject.transform.parent.gameObject;
                startWire = wire;
            }
        }

        if (startParent == null)
            return;

        RecursiveUpdateCurrent(startParent, startWire);
    }

    private void RecursiveUpdateCurrent(GameObject startParent, Wire startWire)
    {
        float startParentCurrent = startParent.GetComponent<Properties>().current;

        foreach (Wire wire in startParent.GetComponent<Properties>().attachedWires)
        {
            if (wire != startWire) //doesnt work since the battery can only have one wire
            {
                //update startParent
                if (startWire.endObject.transform.parent.gameObject != startParent)
                    startParent = startWire.endObject.transform.parent.gameObject;
                else 
                    startParent = startWire.startObject.transform.parent.gameObject;

                //update current
                startParent.GetComponent<Properties>().current = startParentCurrent;

                //update startWire
                startWire = wire;

                RecursiveUpdateCurrent(startParent, startWire);
            }
        }
    }

    private RaycastHit CastRay()
    {
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
