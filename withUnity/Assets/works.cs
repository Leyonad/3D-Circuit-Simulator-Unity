using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

public class WireManager1 : MonoBehaviour
{
    [SerializeField]
    public static Camera cam;
    public int numCapVertices = 4;

    public static Wire selectedWire;

    private List<GameObject> parentsLeft = new List<GameObject>();

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

    private void UpdateElectricityParameters()
    {
        //GameObject startParent = null;
        Wire startWire = null;
        parentsLeft.Clear();

        //find the metal2 object, since that is the start object
        bool found = false;
        foreach (Wire wire in Wire._registry)
        {
            //reset the updated parameter of each wire
            wire.updated = false;

            if (!found)
            {
                if (wire.startObject.transform.parent.name == "Metal2")
                {
                    parentsLeft.Add(wire.endObject.transform.parent.gameObject);
                    //startParent = wire.endObject.transform.parent.gameObject;
                    //startWire = wire;
                    found = true;
                }
                if (wire.endObject.transform.parent.name == "Metal2")
                {
                    parentsLeft.Add(wire.startObject.transform.parent.gameObject);
                    //startParent = wire.startObject.transform.parent.gameObject;
                    //startWire = wire;
                    found = true;
                }
            }
        }
        Debug.Log(" ");
        Debug.Log("START");
        foreach (GameObject parentObject in parentsLeft)
        {
            foreach (Wire wire in parentObject.GetComponent<Properties>().attachedWires)
            {
                if (!wire.updated)
                {
                    RecursiveUpdateCurrent(parentObject, wire);
                }
            }
        }
        Debug.Log("\n PARENTS LEFT LIST ->");
        foreach (GameObject parent in parentsLeft)
            Debug.Log(parent.name);
        Debug.Log("\n <- PARENTS LEFT LIST");
        Debug.Log("END");
        Debug.Log(" ");
    }

    private bool RecursiveUpdateCurrent(GameObject startParent, Wire startWire)
    {
        //stop if no exit wire exists
        int count = 0;
        foreach (Wire wire in startParent.GetComponent<Properties>().attachedWires)
        {
            if (!wire.updated)
                count++;
        }
        //if (startParent.GetComponent<Properties>().attachedWires.Count == 1) {  
        if (count == 1)
        { //one entry
            print("\n STOP ONLY ONE WIRE");
            return false;
        }
        //wire has been visited
        startWire.updated = true;

        bool twoNextWires = false;
        //if (startParent.GetComponent<Properties>().attachedWires.Count > 2) 
        if (count > 2) //one entry, at least two exits
            twoNextWires = true;
        //with updated = false not count > 2

        foreach (Wire wire in startParent.GetComponent<Properties>().attachedWires)
        {
            Debug.Log("\n wirename: " + wire.lineObject.name);

            if (!wire.updated)
            {
                if (!twoNextWires) //meaning one entry, one exit
                {
                    if (parentsLeft.Contains(startParent))
                        parentsLeft.Remove(startParent);
                    print("\n one entry one exit");
                    return RecursiveUpdateCurrent(GetNextObject(startParent, wire), wire);
                }

                //if a wire found and its not the start wire, add the startparent to come back to it later
                print("\n Wire found and added it");
                parentsLeft.Add(startParent);
                return RecursiveUpdateCurrent(GetNextObject(startParent, wire), wire);
            }
            else
            {
                print("\n already updated");
            }
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
