using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Node
{
    public static List<Node> _registry = new List<Node>();
    public static List<Node> _resistorsRegistry = new List<Node>();
    public static List<Node> _voltageSourcesRegistry = new List<Node>();
    public static bool foundGround = false;
    public static Node groundNode;
    public static Node sourceNode;

    public GameObject nodeObject;
    private List<Node> neighborNodes = new List<Node>();
    private List<Node> neighborResistors = new List<Node>();
    private List<Node> neighborVoltageSources = new List<Node>();
    private float voltage;
    private bool ground;
    private bool known = false;

    //matrix stuff
    private static double[,] yMatrix;
    private static double[] iMatrix;

    public Node(GameObject _obj, bool _ground=false, bool _isResistor=false, bool _isVoltageSource=false)
    {
        //dont make duplicates
        foreach (Node node in Node._registry)
            if (node.nodeObject == _obj)
                return;

        nodeObject = _obj;
        ground = _ground;

        if (_isResistor)
            _resistorsRegistry.Add(this);
        else if (_isVoltageSource)
            _voltageSourcesRegistry.Add(this);
        else
            _registry.Add(this);

        //for optimization
        _obj.GetComponent<Properties>().node = this;
    }

    public static void SetNeighborNodes()
    {
        //set the neighbor nodes of each node
        foreach (Node node in Node._registry)
        {
            foreach (Wire wire in node.nodeObject.GetComponent<Properties>().attachedWires)
            {
                GameObject nextObject = WireManager.GetNextObject(node.nodeObject, wire);
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
                    else if (nextObject.GetComponent<Properties>().battery != null)
                    {
                        node.neighborVoltageSources.Add(nextObject.GetComponent<Properties>().node);
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

    public static void CalculateNodes()
    {
        CreateMatrices();
        PrintMatrices();
        CalculateVoltages();
    }

    public static void CreateMatrices()
    {
        int n = _registry.Count + _voltageSourcesRegistry.Count;
        yMatrix = new double[n, n];
        iMatrix = new double[n];
    }

    public static void CalculateVoltages()
    {

    }

    public static void PrintMatrices()
    {
        //print y matrix and i matrix
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < yMatrix.GetLength(1); i++)
        {
            for (int j = 0; j < yMatrix.GetLength(0); j++)
            {
                sb.Append(yMatrix[i, j]);
                sb.Append(' ');
            }
            sb.Append(' ');
            sb.Append(' ');
            sb.Append(' ');
            sb.Append(iMatrix[i]);
            sb.AppendLine();
        }
        Debug.Log("\n" + sb.ToString());
    }

    public static void PrintNodes()
    {
        Debug.Log("START");
        foreach (Node node in Node._registry)
        {
            Debug.Log(node.nodeObject.name + " " + node.nodeObject.transform.position + " ground = " + node.ground + " voltage= " + node.voltage);
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
        _resistorsRegistry.Clear();
        _voltageSourcesRegistry.Clear();
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
