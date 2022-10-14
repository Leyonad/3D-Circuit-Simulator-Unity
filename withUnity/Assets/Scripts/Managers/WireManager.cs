using System.Collections.Generic;
using UnityEngine;

public class WireManager : MonoBehaviour
{
    public int numCapVertices = 4;

    private static List<GameObject> parentsLeft = new List<GameObject>();
    public static List<Wire> connectedWires = new List<Wire>();

    public static bool electricityPathView = false;

    public static void ResetWire()
    {
        //if its a new wire, delete it
        Destroy(Wire.justCreated.lineObject);
    }

    public static void UpdateElectricityParameters()
    {
        NodeManager.ClearAllNodes();

        parentsLeft.Clear(); 
        connectedWires.Clear();

        //find the metal2 object, since that is the start object
        bool found = false;
        GameObject startObject = null;
        Wire startWireNegative = null;
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
                    startObject = wire.startObject.transform.parent.gameObject;
                    startWireNegative = wire;
                    found = true;
                }
                else if (wire.endObject.name == "mn")
                {
                    startObject = wire.endObject.transform.parent.gameObject;
                    startWireNegative = wire;
                    found = true;
                }
            }
        }

        //set the ground node and update the start wire
        if (found)
        {
            new Node(startObject);
            SetWireToVisited(startWireNegative);
            RecursiveUpdateCurrent(GetObjectOfNextNode(startObject, startWireNegative));
        }
    }

    private static bool RecursiveUpdateCurrent(GameObject startParent)
    {
        List<Wire> notVisited = GetNotVisitedWires(startParent);
        int exit = notVisited.Count;

        if (exit == 0)
        { //---------------no exit-------------------
            new Node(startParent);
            parentsLeft.Remove(startParent);
            
            //stop if there are no parent objects left
            if (parentsLeft.Count == 0)
                return false;

            //find the next startParent
            GameObject nextStartParent = parentsLeft[0];
            for (int i = 0; i < nextStartParent.GetComponent<Properties>().attachedWires.Count; i++)
            {
                Wire wire = nextStartParent.GetComponent<Properties>().attachedWires[i];
                if (!wire.updated)
                {
                    SetWireToVisited(wire);
                    
                    //remove this startparent, if all of its attached wires are updated
                    if (GetNotVisitedWires(nextStartParent).Count == 0) 
                        parentsLeft.Remove(nextStartParent);
                    
                    return RecursiveUpdateCurrent(GetObjectOfNextNode(nextStartParent, wire));
                }
            }
        }

        //make a new node 
        new Node(startParent);

        foreach (Wire wire in notVisited)
        {
            SetWireToVisited(wire);

            if (exit == 1) //------------one exit------------
            {
                parentsLeft.Remove(startParent);
                return RecursiveUpdateCurrent(GetObjectOfNextNode(startParent, wire));
            }

            //---------------multiple exits---------------
            if (!parentsLeft.Contains(startParent))
                parentsLeft.Add(startParent);

            return RecursiveUpdateCurrent(GetObjectOfNextNode(startParent, wire));
            
        }
        return false;
    }

    private static void SetWireToVisited(Wire wire)
    {
        if (electricityPathView)
            wire.lineRenderer.material = ResourcesManager.yellow;
        connectedWires.Add(wire);
        wire.updated = true;
    }

    private static List<Wire> GetNotVisitedWires(GameObject startParent)
    {
        List<Wire> notVisited = new();
        foreach (Wire wire in startParent.GetComponent<Properties>().attachedWires)
            if (!wire.updated)
                notVisited.Add(wire);
        
        return notVisited;
    }

    public static GameObject GetObjectOfNextNode(GameObject startParent, Wire wire)
    {
        //find the gameobject which the other end of the wire is attached to (skip an item)
        if (wire.startObject.transform.parent.gameObject == startParent)
        {
            if (IsItem(wire.endObject.transform.parent.gameObject))
                return GetObjectAfterItem(wire, wire.endObject.transform.parent.gameObject);
            return wire.endObject.transform.parent.gameObject;
        }

        if (IsItem(wire.startObject.transform.parent.gameObject))
            return GetObjectAfterItem(wire, wire.startObject.transform.parent.gameObject);
        return wire.startObject.transform.parent.gameObject;
    }

    public static GameObject GetNextMetal(GameObject startParent, Wire wire)
    {
        //find the metal which the other end of the wire is attached to
        if (wire.startObject.transform.parent.gameObject == startParent)
            return wire.endObject;

        return wire.startObject;
    }

    public static GameObject GetObjectAfterItem(Wire wireToItem, GameObject itemObject)
    {
        //find the gameobject which is at the other end of the itemObject
        foreach (Wire wire in itemObject.GetComponent<Properties>().attachedWires)
        {
            if (wire != wireToItem)
            {
                return GetObjectOfNextNode(itemObject, wire);
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

    public static Node GetNodeAfterWire(Node startNode, Wire wire)
    {
        //this method returns the next node after a wire
        return GetObjectOfNextNode(startNode.nodeObject, wire).GetComponent<Properties>().node;
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

    public static Wire GetWireBetweenTwoGameObjects(GameObject obj1, GameObject obj2)
    {
        foreach (Wire wire in obj1.GetComponent<Properties>().attachedWires)
            foreach (Wire wire2 in obj2.GetComponent<Properties>().attachedWires)
                if (wire == wire2)
                    return wire;
        return null;
    }
}
