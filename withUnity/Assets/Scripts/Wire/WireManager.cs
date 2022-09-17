using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WireManager : MonoBehaviour
{
    public static Camera cam;
    public static GameObject led;
    public int numCapVertices = 4;

    public static Wire selectedWire;

    private List<GameObject> parentsLeft = new List<GameObject>();

    private void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        led = GameObject.FindGameObjectWithTag("LED");
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

    private void UpdateElectricityParameters()
    {
        parentsLeft.Clear();
        led.GetComponent<MeshRenderer>().material = ResourcesManager.white;
        //Debug.Log("\n START");

        //find the metal2 object, since that is the start object
        bool found = false;
        foreach (Wire wire in Wire._registry)
        {
            //reset the updated parameter of each wire
            wire.updated = false;
            wire.lineRenderer.material = ResourcesManager.wireMaterial;

            if (!found)
            {
                if (wire.startObject.transform.parent.name == "Metal2")
                {
                    parentsLeft.Add(wire.startObject.transform.parent.gameObject);
                    found = true;
                }
                if (wire.endObject.transform.parent.name == "Metal2")
                {
                    parentsLeft.Add(wire.endObject.transform.parent.gameObject);
                    found = true;
                }
            }
        }
        for (int i = 0; i < parentsLeft.Count; i++)
        {
            foreach (Wire wire in parentsLeft[i].GetComponent<Properties>().attachedWires)
            {
                if (!wire.updated)
                {
                    wire.updated = true;
                    wire.lineRenderer.material = ResourcesManager.yellow;
                    RecursiveUpdateCurrent(GetNextObject(parentsLeft[i], wire), wire);
                }
            }
            //print("next parentobject: " + parentsLeft[i].transform.GetInstanceID());
        }
    }

    private bool RecursiveUpdateCurrent(GameObject startParent, Wire startWire)
    {
        //print("startWire: " + startWire.lineObject.name);
        //print("startParent: " + startParent.transform.GetInstanceID());

        int exit = 0;
        List<Wire> notVisited = new List<Wire>();
        foreach (Wire wire in startParent.GetComponent<Properties>().attachedWires) {
            if (!wire.updated)
            {
                exit++;
                notVisited.Add(wire);
            }
        }

        if (exit == 0) { //---------------no exit-------------------
            //print("exit = 0");
            if (startParent.name == "Metal1")
            {
                led.GetComponent<MeshRenderer>().material = ResourcesManager.red;
                Debug.Log("CIRCUIT COMPLETE");
            }
            parentsLeft.Remove(startParent);
            return false;
        }

        foreach (Wire wire in notVisited)
        {
            wire.lineRenderer.material = ResourcesManager.yellow;
            wire.updated = true;
            if (exit == 1) //------------one exit------------
            {
                //print("exit = 1");
                parentsLeft.Remove(startParent);
                return RecursiveUpdateCurrent(GetNextObject(startParent, wire), wire);
            }

            //---------------multiple exits---------------
            //print("multiple exits");
            parentsLeft.Add(startParent);
            return RecursiveUpdateCurrent(GetNextObject(startParent, wire), wire);
            
        }
        return false;
    }

    private GameObject GetNextObject(GameObject startParent, Wire wire)
    {
        if (wire.startObject.transform.parent.gameObject == startParent)
            return wire.endObject.transform.parent.gameObject;

        return wire.startObject.transform.parent.gameObject;
    }

    public static void SelectWire(Wire wire)
    {
        selectedWire = wire;
        //selectedWire.lineRenderer.material = ResourcesManager.highlightWireMaterial;
    }

    public static void UnselectWire()
    {
        if (selectedWire == null)
            return;
        //selectedWire.lineRenderer.material = ResourcesManager.wireMaterial;
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
