using System.Collections.Generic;
using UnityEngine;

public class WireManager : MonoBehaviour
{
    public int numCapVertices = 4;

    private static List<GameObject> parentsLeft = new List<GameObject>();
    public static List<Wire> connectedWires = new List<Wire>();

    public static bool electricityPathView = false;
    public static bool circuitComplete = false;

    public static void ResetWire()
    {
        //if its a new wire, delete it
        Destroy(Wire.justCreated.lineObject);
    }

    public static void UpdateElectricityParameters()
    {
        Node.ClearAllNodes();
        Node.foundGround = false;
        Node.groundNode = null;

        parentsLeft.Clear(); 
        connectedWires.Clear();
        circuitComplete = false;

        //find the metal2 object, since that is the start object
        bool found = false;
        Wire startWireNegative = null;
        GameObject groundNodeGameObject = null;
        foreach (Wire wire in Wire._registry)
        {
            //reset the updated-parameter of each wire
            wire.updated = false;
            wire.lineRenderer.material = wire.wireColor;

            if (!found)
            {
                //add the battery to parentsLeft and set the ground node
                if (wire.startObject.name == "mn")
                {
                    parentsLeft.Add(wire.startObject.transform.parent.gameObject);
                    groundNodeGameObject = GetNextObject(parentsLeft[0], wire);
                    startWireNegative = wire;
                    found = true;
                }
                else if (wire.endObject.name == "mn")
                {
                    parentsLeft.Add(wire.endObject.transform.parent.gameObject);
                    groundNodeGameObject = GetNextObject(parentsLeft[0], wire);
                    startWireNegative = wire;
                    found = true;
                }
            }
        }

        //set the ground node and update the start wire
        if (found)
        {
            Node.foundGround = true;
            Node ground = new Node(groundNodeGameObject, true);
            Node.groundNode = ground;
            groundNodeGameObject.GetComponent<Properties>().ground = true;

            startWireNegative.updated = true;
            if (electricityPathView)
                startWireNegative.lineRenderer.material = ResourcesManager.yellow;
            connectedWires.Add(startWireNegative);

            //start with the battery as the first startParent
            RecursiveUpdateCurrent(GetNextObject(parentsLeft[0], startWireNegative));
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
            //else Debug.Log($"WIRE ALREDY UPDATED: {wire.lineObject.name}");
        }
        //Debug.Log($"startparent = {startParent.name} {startParent.transform.position} exits: {exit}");
        if (exit == 0)
        { //---------------no exit-------------------
            if (startParent.name == "Battery9V(Clone)")
            {
                //make a voltage source node for the battery
                new Node(startParent, false, false, true);
                circuitComplete = true;
                Debug.Log("CIRCUIT COMPLETE");
            }
            //also make a node for a metalstrip if there are no exits (wire to a random metalstrip) 
            /*else if (startParent.GetComponent<Properties>().attachedWires.Count == 0)
            {
                new Node(startParent);
            }*/
            parentsLeft.Remove(startParent);

            //stop if there are no parent objects left
            if (parentsLeft.Count == 0) 
                return false;

            //find the next startParent
            foreach (Wire wire in parentsLeft[parentsLeft.Count-1].GetComponent<Properties>().attachedWires)
            {
                if (!wire.updated)
                {
                    if (electricityPathView)
                        wire.lineRenderer.material = ResourcesManager.yellow;
                    connectedWires.Add(wire);
                    wire.updated = true;
                    //Debug.Log($"UPDATE WIRE NOW {wire.lineObject.name} startParent = {startParent.name} {startParent.transform.position}");
                    return RecursiveUpdateCurrent(GetNextObject(parentsLeft[0], wire));
                }
            }  
        }

        if (IsItem(startParent) && startParent.GetComponent<Properties>().battery == null)
        {
            Item item = startParent.GetComponent<Properties>().item;
            if (item.itemObject.name == "LED")
            {
                new Node(startParent, false, false, false, true);
            }
            else if (item.itemObject.name == "Resistor")
            {
                new Node(startParent, false, true);
            }
        }
        //make a new node if the metal is not ground
        else if (startParent != Node.groundNode.nodeObject)
            new Node(startParent);

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
