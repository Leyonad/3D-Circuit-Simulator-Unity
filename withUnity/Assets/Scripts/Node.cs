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

    //matrix stuff
    private static List<Node> unknownNodes = new List<Node>();
    private static int matrixDimension;
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

        //set the neighbor nodes of each voltage source
        foreach (Node voltageSourceNode in Node._voltageSourcesRegistry)
        {
            foreach (Wire wire in voltageSourceNode.nodeObject.GetComponent<Properties>().attachedWires)
            {
                GameObject nextObject = WireManager.GetNextObject(voltageSourceNode.nodeObject, wire);
                voltageSourceNode.neighborNodes.Add(nextObject.GetComponent<Properties>().node);
            }
        }
    }

    public static void CalculateNodes()
    {
        unknownNodes = _registry;
        unknownNodes.Remove(GetGroundNode());
        matrixDimension = unknownNodes.Count + _voltageSourcesRegistry.Count;

        CreateMatrices();
        AssignValuesToMatrices();
        PrintMatrices();
        CalculateInverseMatrix();
        PrintMatrices();
    }

    public static void CreateMatrices()
    {
        int n = matrixDimension;
        yMatrix = new double[n,n];
        iMatrix = new double[n];
    }

    public static void CalculateInverseMatrix()
    {
        //make a copy of the matrix as a jagged array
        double[][] jaggedMatrix = new double[matrixDimension][];
        for (int i = 0; i < matrixDimension; i++)
        {
            jaggedMatrix[i] = new double[matrixDimension];
            for (int j = 0; j < matrixDimension; j++)
            {
                jaggedMatrix[i][j] = yMatrix[i, j];
            }
        }

        //calculate the inverse matrix of the jagged array
        double[][] invertedJaggedMatrix = Matrix.MatrixInverse(jaggedMatrix);
        
        //convert the inverse matrix back to a multidimensional array
        for (int i = 0; i < matrixDimension; i++)
            for (int j = 0; j < matrixDimension; j++)
                yMatrix[i, j] = invertedJaggedMatrix[i][j];
    }

    public static void AssignValuesToMatrices()
    {
        //assign values to the matrices
        for (int i = 0; i < unknownNodes.Count; i++)
        {
            //assign neighbor resistor values
            foreach (Node neighborResistorNode in unknownNodes[i].neighborResistors)
            {
                double resistance = neighborResistorNode.nodeObject.GetComponent<Properties>().resistance;
                int indexNeighborNode = GetIndexOfNodeAfterResistor(unknownNodes[i], neighborResistorNode);

                //plus neighbor resistor at [i, i]
                yMatrix[i, i] += 1 / resistance;

                //minus neighbor resistor at [i, index of neighbor node]
                //only if the resistor is between two normal nodes (not ground)
                if (indexNeighborNode > -1)
                {
                    yMatrix[i, indexNeighborNode] -= 1 / resistance;
                }
            }
        }

        //handle voltage sources
        for (int i = 0; i < _voltageSourcesRegistry.Count; i++)
        {
            double voltage = _voltageSourcesRegistry[i].nodeObject.GetComponent<Properties>().voltage;
            
            foreach (Node neighborNode in _voltageSourcesRegistry[i].neighborNodes)
            {
                if (!neighborNode.ground)
                {
                    //get the index of the node which the voltage source is connected to
                    int index = unknownNodes.IndexOf(neighborNode);

                    yMatrix[index, matrixDimension - (i + 1)] = 1;
                    yMatrix[matrixDimension - (i + 1), index] = 1;

                    iMatrix[matrixDimension - (i + 1)] = voltage;
                }
            }
        }
    }

    public static int GetIndexOfNodeAfterResistor(Node startNode, Node resistorNode)
    {
        //get the index of the node after the resistor, -1 if not found
        foreach (Node neighborNode in startNode.neighborNodes)
        {
            foreach (Node neighborResistorNode in neighborNode.neighborResistors)
            {
                if (neighborResistorNode == resistorNode)
                {
                    //if the node after the resistor is the ground node: return -1
                    if (neighborNode.ground == true)
                    {
                        return -1;
                    }

                    return unknownNodes.IndexOf(neighborNode);
                }
            }
        }
        return -1;
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
                sb.Append(' ');
            }
            sb.Append(' ');
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

    public static Node GetGroundNode()
    //get the node connected to the negative side of the battery
    {
        return groundNode;
    }
}
