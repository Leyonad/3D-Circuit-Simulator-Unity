using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public static List<Node> _registry = new List<Node>();
    public static List<Node> _resistorsRegistry = new List<Node>();
    public static bool foundGround = false;
    public static Node groundNode;
    public static Node sourceNode;

    public GameObject nodeObject;
    private List<Node> neighborNodes = new List<Node>();
    private List<Node> neighborResistors = new List<Node>();
    private float voltage;
    private bool ground;
    private bool known = false;

    public Node(GameObject _obj, bool _ground=false, bool _isResistor=false)
    {
        nodeObject = _obj;
        ground = _ground;

        if (!_isResistor)
            _registry.Add(this);
        else
            _resistorsRegistry.Add(this);

        //for optimization
        _obj.GetComponent<Properties>().node = this;
    }

    public static void SetNeighborNodes()
    {
        //set the neighbor nodes of each node
        foreach (Node node in _registry)
        {
            foreach (Wire wire in node.nodeObject.GetComponent<Properties>().attachedWires)
            {
                GameObject nextObject = WireManager.GetNextObject(node.nodeObject, wire);

                //set the ground and source node as neighbors (if not already connected)
                if (nextObject.CompareTag("mP"))
                {
                    if (!node.neighborNodes.Contains(GetGroundNode()))
                        node.neighborNodes.Add(GetGroundNode());
                    continue;
                }

                if (nextObject.CompareTag("mN"))
                {
                    if (!node.neighborNodes.Contains(GetSourceNode()))
                        node.neighborNodes.Add(GetSourceNode());
                    continue;
                }

                if (WireManager.IsItem(nextObject))
                {
                    //set the object at the other end of an item as the neighbor
                    GameObject objectAfterItem = WireManager.GetObjectAfterItem(wire, nextObject);
                    node.neighborNodes.Add(objectAfterItem.GetComponent<Properties>().node);

                    //add the resistor to the neighborResistors list
                    if (nextObject.name == "Resistor")
                    {
                        node.neighborResistors.Add(nextObject.GetComponent<Properties>().node);
                    }
                    continue;
                }

                //set normal neighbor nodes
                Node neighborNode = nextObject.GetComponent<Properties>().node;
                if (neighborNode != null && !node.neighborNodes.Contains(neighborNode)) 
                    //.Contains(): not efficient since this can only be the case at the wires nearby the battery
                    node.neighborNodes.Add(neighborNode);
            }
        }
    }

    public static void PrintNodes()
    {
        Debug.Log("START");
        foreach (Node node in Node._registry)
        {
            Debug.Log(node.nodeObject.name + " " + node.nodeObject.transform.position + " ground= " + node.ground + " voltage= " + node.voltage);
        }
        Debug.Log("END");
    }

    public static void PrintNeighbors()
    {
        Debug.Log("START");
        foreach (Node node in Node._registry)
        {
            Debug.Log(node.nodeObject.name + "  " + node.nodeObject.transform.position + ":");
            foreach (Node neighborNode in node.neighborNodes)
            {
                if (neighborNode != null)
                    Debug.Log("   Neighbor: " + neighborNode.nodeObject.name + " " + neighborNode.nodeObject.transform.position);
                else Debug.Log("   NULL");
            }
        }
        Debug.Log("END");
    }

    public static void PrintNeighborResistors()
    {
        Debug.Log("START");
        foreach (Node node in Node._registry)
        {
            Debug.Log(node.nodeObject.name + "  " + node.nodeObject.transform.position + ":");
            foreach (Node neighborResistor in node.neighborResistors)
            {
                if (neighborResistor != null)
                    Debug.Log("   RESISTOR: " + neighborResistor.nodeObject.name + " " + neighborResistor.nodeObject.transform.position);
                else Debug.Log("   NULL");
            }
        }
        Debug.Log("END");
    }

    public static void ClearAllNodes()
    {
        _registry.Clear();
    }

    public void UpdateVoltageOfNode(float _voltage)
    {
        voltage = _voltage;
    }

    public void SetToKnown()
    {
        known = true;
    }

    public void SetToUnknown()
    {
        known = false;
    }

    public static Node GetGroundNode()
    //get the node connected to the negative side of the battery
    {
        return groundNode;
    }

    public static Node GetSourceNode()
    //get the node connected to the positive side of the battery
    {
        return sourceNode;
    }
}
