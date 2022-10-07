using System.Collections.Generic;
using UnityEngine;

public class WireManager : MonoBehaviour
{
    public int numCapVertices = 4;

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
        GameObject groundNodeGameObject = null;
        foreach (Wire wire in Wire._registry)
        {
            //reset the updated-parameter of each wire
            wire.updated = false;
            wire.lineRenderer.material = wire.wireColor;

            if (!found)
            {
                if (wire.startObject.name == "mn")
                {
                    parentsLeft.Add(wire.startObject.transform.parent.gameObject);
                    groundNodeGameObject = GetNextObject(parentsLeft[0], wire);
                    found = true;
                }
                else if (wire.endObject.name == "mn")
                {
                    parentsLeft.Add(wire.endObject.transform.parent.gameObject);
                    groundNodeGameObject = GetNextObject(parentsLeft[0], wire);
                    found = true;
                }
            }
        }

        //set the ground node
        if (found)
        {
            Node.foundGround = true;
            Node ground = new Node(groundNodeGameObject, true);
            Node.groundNode = ground;
            ground.SetToKnown();
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
            if (startParent.name == "Battery9V(Clone)")
            {
                Debug.Log("CIRCUIT COMPLETE");
                circuitComplete = true;
            }
            //also make a node for a metalstrip if there are no exits (wire to a random metalstrip) 
            else
            {
                new Node(startParent);
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
            {
                connectedResistors.Add(item);
                new Node(startParent, false, true);
            }
        }
        //add this startparent to the node list by making a new node
        else if (startParent != Node.groundNode.nodeObject)
        {
            new Node(startParent);
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

    public static GameObject GetNextObject(GameObject startParent, Wire wire)
    {
        //find the gameobject to which the other end of the wire is attached to
        if (wire.startObject.transform.parent.gameObject == startParent)
            return wire.endObject.transform.parent.gameObject;

        return wire.startObject.transform.parent.gameObject;
    }

    public static GameObject GetObjectAfterItem(Wire wireToItem, GameObject itemObject)
    {
        //find the gameobject which is at the other end of the itemObject
        foreach (Wire wire in itemObject.GetComponent<Properties>().attachedWires)
        {
            if (wire != wireToItem)
            {
                return GetNextObject(itemObject, wire);
            }
        }
        return null;
    }

    public static Item GetItemAttachedToWire(Wire wire)
    {
        if (wire == null) return null;

        if (wire.startObject.transform.parent.CompareTag("Item"))
            return wire.startObject.transform.parent.GetComponent<Properties>().item;
        else if (wire.endObject.transform.parent.CompareTag("Item"))
            return wire.endObject.transform.parent.GetComponent<Properties>().item;

        return null;
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
