using System.Collections.Generic;
using UnityEngine;

public class WireManager : MonoBehaviour
{
    public int numCapVertices = 4;

    public static Wire selectedWire;
    public static Material selectedWirePreviousMaterial;
    public static Material selectedWireMetalPreviousMaterialStart;
    public static Material selectedWireMetalPreviousMaterialEnd;

    private static List<GameObject> parentsLeft = new List<GameObject>();
    private static List<Item> connectedItems = new List<Item>();
    private static bool circuitComplete = false;

    public static bool canClickThisFrame = true;

    public static void ResetWire()
    {
        //if its a new wire, delete it
        Destroy(Wire.justCreated.lineObject);
    }

    public static void UpdateElectricityParameters()
    {
        //reset the material of the previously connected LEDs
        foreach (Item item in connectedItems)
            item.itemObject.GetComponent<MeshRenderer>().material = ResourcesManager.LED_default;

        parentsLeft.Clear();
        connectedItems.Clear();
        circuitComplete = false;

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
                else if (wire.endObject.transform.parent.name == "Metal2")
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
        //make LEDs glow
        if (circuitComplete)
        {
            foreach (Item item in connectedItems)
            {
                item.itemObject.GetComponent<MeshRenderer>().material = item.ledColor;
            }
        }
        
    }

    private static bool RecursiveUpdateCurrent(GameObject startParent)
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

        //additionally add an item to a seperate list
        if (IsItem(startParent))
        {
            connectedItems.Add(startParent.GetComponent<Properties>().item);
        }

        if (exit == 0) { //---------------no exit-------------------
            if (startParent.name == "Metal1")
            {
                Debug.Log("CIRCUIT COMPLETE");
                circuitComplete = true;
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

    private static GameObject GetNextObject(GameObject startParent, Wire wire)
    {
        if (wire.startObject.transform.parent.gameObject == startParent)
            return wire.endObject.transform.parent.gameObject;

        return wire.startObject.transform.parent.gameObject;
    }

    public static void SelectWire(Wire wire)
    {
        if (wire == null) return;

        UnselectWire();
        selectedWire = wire;
        SaveCurrentMaterialNotHighlighted();
        SetMaterialHighlight();
    }

    public static void UnselectWire()
    {
        if (selectedWire == null) return;

        ResetMaterialHighlight();
        selectedWire = null;
    }
    public static void SetMaterialHighlight()
    {
        if (selectedWire == null) return;

        selectedWire.lineRenderer.material = ResourcesManager.highlightWireMaterial;
        selectedWire.startObject.GetComponent<MeshRenderer>().material = ResourcesManager.highlightWireMaterial;
        selectedWire.endObject.GetComponent<MeshRenderer>().material = ResourcesManager.highlightWireMaterial;
    }

    public static void SaveCurrentMaterialNotHighlighted()
    {
        if (selectedWire == null) return;

        selectedWirePreviousMaterial = selectedWire.lineRenderer.material;
        selectedWireMetalPreviousMaterialStart = selectedWire.startObject.GetComponent<MeshRenderer>().material;
        selectedWireMetalPreviousMaterialEnd = selectedWire.endObject.GetComponent<MeshRenderer>().material;
    }

    public static void ResetMaterialHighlight()
    {
        if (selectedWire == null) return;

        selectedWire.lineRenderer.material = selectedWirePreviousMaterial;
        selectedWire.startObject.GetComponent<MeshRenderer>().material = selectedWireMetalPreviousMaterialStart;
        selectedWire.endObject.GetComponent<MeshRenderer>().material = selectedWireMetalPreviousMaterialEnd;
        selectedWirePreviousMaterial = null;
        selectedWireMetalPreviousMaterialStart = null;
        selectedWireMetalPreviousMaterialEnd = null;
    }

    public static bool IsMetal(GameObject obj)
    {
        if (obj == null) return false;

        if (obj.CompareTag("Metal"))
            return true;
        return false;
    }

    public static bool IsItem(GameObject obj)
    {
        if (obj == null) return false;

        if (obj.CompareTag("Item")) 
            return true;
        return false;
    }

    public static bool IsWire(Collider collider)
    {
        if (collider == null) return false;

        if (collider.GetComponent<LineRenderer>() != null)
            return true;
        return false;
    }

    public static bool IsAttachedToItem(Wire wire)
    {
        if (wire == null) return false;
        return wire.startObject.transform.parent.CompareTag("Item") || 
            wire.endObject.transform.parent.CompareTag("Item");
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
