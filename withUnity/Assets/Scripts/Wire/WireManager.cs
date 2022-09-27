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
    public static List<Wire> connectedWires = new List<Wire>();
    private static List<Item> connectedLeds = new List<Item>();
    private static List<Item> connectedResistors = new List<Item>();
    private static bool circuitComplete = false;

    public static bool electricityPathView = false;

    public static void ResetWire()
    {
        //if its a new wire, delete it
        Destroy(Wire.justCreated.lineObject);
    }

    public static void UpdateElectricityParameters()
    {
        //reset the material of the previously connected LEDs
        foreach (Item item in connectedLeds)
            item.itemObject.GetComponent<MeshRenderer>().material = ResourcesManager.LED_default;

        Node.ClearAllNodes();
        Node.foundGround = false;

        parentsLeft.Clear(); 
        connectedWires.Clear();
        connectedLeds.Clear();
        connectedResistors.Clear();
        circuitComplete = false;

        //find the metal2 object, since that is the start object
        bool found = false;
        GameObject battery = null;
        foreach (Wire wire in Wire._registry)
        {
            //reset the updated-parameter of each wire
            wire.updated = false;
            wire.lineRenderer.material = wire.wireColor;

            if (!found)
            {
                if (wire.startObject.transform.parent.name == "Metal1")
                {
                    parentsLeft.Add(wire.startObject.transform.parent.gameObject);
                    battery = wire.startObject.transform.parent.gameObject;
                    found = true;
                }
                else if (wire.endObject.transform.parent.name == "Metal1")
                {
                    parentsLeft.Add(wire.endObject.transform.parent.gameObject);
                    battery = wire.endObject.transform.parent.gameObject;
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
                    if (electricityPathView)
                        wire.lineRenderer.material = ResourcesManager.yellow;
                    connectedWires.Add(wire);
                    wire.updated = true;
                    RecursiveUpdateCurrent(GetNextObject(parentsLeft[i], wire));
                }
            }
        }
        //make LEDs glow
        if (circuitComplete)
        {

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

        if (exit == 0)
        { //---------------no exit-------------------
            if (startParent.name == "Metal2")
            {
                Debug.Log("CIRCUIT COMPLETE");
                circuitComplete = true;

                //set voltage of node connected to the positive side of the battery
                GameObject obj = GetNextObject(startParent, startParent.GetComponent<Properties>().attachedWires[0]);
                foreach (Node node in Node._registry)
                    if (node.nodeObject == obj)
                        node.UpdateVoltageOfNode(startParent.GetComponent<Properties>().voltage);
            }
            parentsLeft.Remove(startParent);
            return false;
        }

        //additionally add an item to a seperate list
        if (IsItem(startParent))
        {
            Item item = startParent.GetComponent<Properties>().item;
            if (item.itemObject.name == "LED")
                connectedLeds.Add(item);
            else if (item.itemObject.name == "Resistor")
                connectedResistors.Add(item);
        }
        //add this startparent to the node list by making a new node
        else if (Node.foundGround)
        {
            new Node(startParent);
        }
        //if not found the ground yet, make this object the ground, since its always the
        //start/endobject of the metal1 wire
        else
        {
            Node.foundGround = true;
            new Node(startParent, true);
        }

        foreach (Wire wire in notVisited)
        {
            if (electricityPathView)
                wire.lineRenderer.material = ResourcesManager.yellow;
            connectedWires.Add(wire);
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
        //find the gameobject to which the other end of the wire is attached to
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
