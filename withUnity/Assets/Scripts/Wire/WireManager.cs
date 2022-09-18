using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WireManager : MonoBehaviour
{
    public static Camera cam;
    public int numCapVertices = 4;

    public static Wire selectedWire;
    public static Material selectedWirePreviousMaterial;

    private List<GameObject> parentsLeft = new List<GameObject>();

    public static bool canClickThisFrame = true;

    private void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    private void Update()
    {
        canClickThisFrame = true;
        if (Wire.justCreated != null)
        {
            Wire.justCreated.WireFollowMouse(Wire.justCreated);

            //cancel creation of a new wire
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                ResetWire();
                Wire.justCreated = null;
                return;
            }
            //finish creation of a new wire by adding it to _registry
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                canClickThisFrame = false;

                //check if clicked on a gameobject
                RaycastHit hit = MouseInteraction.CastRay();

                bool wirePossible = false;
                if (hit.collider != null)
                    if (IsMetal(hit.collider.gameObject))
                        if (hit.collider.gameObject != Wire.justCreated.startObject)
                            if (GetWireInMetal(hit.collider.gameObject) == null)
                                wirePossible = true;

                if (wirePossible)
                {
                    Wire.justCreated.endObject = hit.collider.gameObject;
                    
                    Wire.justCreated.startObject.transform.parent.gameObject.GetComponent<Properties>().attachedWires.Add(Wire.justCreated);
                    Wire.justCreated.endObject.transform.parent.gameObject.GetComponent<Properties>().attachedWires.Add(Wire.justCreated);

                    Wire.justCreated.lineRenderer.SetPosition(Wire.justCreated.verticesAmount - 1, hit.collider.gameObject.transform.position);
                    Wire.justCreated.UpdateLinesOfWire();
                    Wire.justCreated.UpdateMeshOfWire();
                    Wire._registry.Add(Wire.justCreated);

                    //Update the electricity parameters of all wires
                    UpdateElectricityParameters();
                    Wire.justCreated = null;
                }
            }
        }

        //delete wire when pressing delete-key
        else if (Keyboard.current.deleteKey.wasPressedThisFrame)
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
    }

    private void ResetWire()
    {
        //if its a new wire, delete it
        Destroy(Wire.justCreated.lineObject);
    }

    private void UpdateElectricityParameters()
    {
        parentsLeft.Clear();

        //find the metal2 object, since that is the start object
        bool found = false;
        foreach (Wire wire in Wire._registry)
        {
            //reset the updated-parameter of each wire
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
                    RecursiveUpdateCurrent(GetNextObject(parentsLeft[i], wire));
                }
            }
        }
    }

    private bool RecursiveUpdateCurrent(GameObject startParent)
    {
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
            if (startParent.name == "Metal1")
            {
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
                parentsLeft.Remove(startParent);
                return RecursiveUpdateCurrent(GetNextObject(startParent, wire));
            }

            //---------------multiple exits---------------
            parentsLeft.Add(startParent);
            return RecursiveUpdateCurrent(GetNextObject(startParent, wire));
            
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
        if (wire == null)
            return;
        UnselectWire();
        selectedWire = wire;
        selectedWirePreviousMaterial = wire.lineRenderer.material;
        selectedWire.lineRenderer.material = ResourcesManager.highlightWireMaterial;
    }

    public static void UnselectWire()
    {
        if (selectedWire == null)
            return;
        selectedWire.lineRenderer.material = selectedWirePreviousMaterial;
        selectedWire = null;
        selectedWirePreviousMaterial = null;
    }

    public static bool IsMetal(GameObject obj)
    {
        if (obj == null) return false;

        if (obj.CompareTag("Metal"))
            return true;
        return false;
    }

    public static Wire GetWireInMetal(GameObject metal)
    {
        foreach (Wire existingWire in Wire._registry)
        {
            if (metal == existingWire.startObject
                || metal == existingWire.endObject
                || metal == existingWire.startObject
                || metal == existingWire.endObject)
            {
                //Debug.Log("WIRE ALREADY EXISTS!");
                return existingWire;
            }
        }
        return null;
    }
}
