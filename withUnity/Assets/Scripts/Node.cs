using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Node
{
    public static List<Node> _registry = new List<Node>();
    public static List<Node> _resistorsRegistry = new List<Node>();
    public static List<Node> _ledRegistry = new List<Node>();
    public static List<Node> _voltageSourcesRegistry = new List<Node>();
    public static List<Node[]> _neighborShortcircuitRegistry = new List<Node[]>();

    public static bool foundGround = false;
    public static Node groundNode;
    public static Node sourceNode;

    public GameObject nodeObject;
    private readonly List<Node> neighborNodes = new List<Node>();
    private readonly List<Node> neighborResistors = new List<Node>();
    private readonly List<Node> neighborLeds = new List<Node>();
    private readonly List<Node> neighborVoltageSources = new List<Node>();
    private readonly List<Node> neighborShortCircuits = new List<Node>();
    private bool ground;

    //matrix stuff
    private static List<Node> unknownNodes = new List<Node>();
    private static int matrixDimension;
    private static double[][] yMatrix;
    private static double[][] iMatrix;
    private static double[][] resultMatrix;

    public Node(GameObject _obj, bool _ground=false, bool _isResistor=false, bool _isVoltageSource=false, bool _isLED=false)
    {
        //dont make duplicate nodes
        foreach (Node node in Node._registry)
            if (node.nodeObject == _obj)
                return;

        nodeObject = _obj;
        ground = _ground;

        if (_isResistor)
            _resistorsRegistry.Add(this);
        else if (_isVoltageSource)
            _voltageSourcesRegistry.Add(this);
        else if (_isLED)
            _ledRegistry.Add(this);
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

                    //resistor
                    if (nextObject.name == "Resistor")
                        node.neighborResistors.Add(nextObject.GetComponent<Properties>().node);

                    //led
                    else if (nextObject.name == "LED")
                        node.neighborLeds.Add(nextObject.GetComponent<Properties>().node);

                    //voltage source
                    else if (nextObject.GetComponent<Properties>().battery != null)
                        node.neighborVoltageSources.Add(nextObject.GetComponent<Properties>().node);

                    continue;
                }

                //set normal neighbor nodes
                Node neighborNode = nextObject.GetComponent<Properties>().node;

                //continue if a wire has both ends on the same node
                if (neighborNode == node) 
                    continue;

                if (neighborNode != null && !node.neighborNodes.Contains(neighborNode))
                {
                    //.Contains(): not efficient since this can only be the case at the wires nearby the battery
                    node.neighborShortCircuits.Add(neighborNode);

                    //add the start and the end node of the short circuit
                    Node[] tempShortcircuit = new Node[2] { node, neighborNode };
                    bool connectionAlreadyInRegistry = false;
                    foreach (Node[] checkArray in _neighborShortcircuitRegistry)
                    {
                        if (checkArray[0] == tempShortcircuit[1] && checkArray[1] == tempShortcircuit[0])
                        {
                            connectionAlreadyInRegistry = true;
                            break;
                        }
                    }
                    if (!connectionAlreadyInRegistry)
                        _neighborShortcircuitRegistry.Add(tempShortcircuit);
                }

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
        matrixDimension = unknownNodes.Count + _neighborShortcircuitRegistry.Count + _voltageSourcesRegistry.Count + _ledRegistry.Count;
        
        CreateMatrices();
        AssignValuesToMatrices();
        PrintMatrix("yMatrix", yMatrix);
        PrintMatrix("iMatrix", iMatrix);

        CalculateInverseMatrix();
        CalculateResultMatrix();
        PrintMatrix("resultMatrix", resultMatrix);

        AssignResultVoltagesToNodes();
        //PrintNodes();
    }

    public static void AssignResultVoltagesToNodes()
    {
        for (int i = 0; i < unknownNodes.Count; i++)
        {
            unknownNodes[i].nodeObject.GetComponent<Properties>().voltage = resultMatrix[i][0];
        }
    }

    public static void CreateMatrices()
    {
        int n = matrixDimension;
        
        //make the y and i matrix
        yMatrix = new double[n][];
        iMatrix = new double[n][];
        for (int i = 0; i < n; i++)
        {
            yMatrix[i] = new double[matrixDimension];
            iMatrix[i] = new double[1];
        } 
    }

    public static void CalculateResultMatrix()
    {
        resultMatrix = Matrix.MatrixProduct(yMatrix, iMatrix);
    }

    public static void CalculateInverseMatrix()
    {
        //calculate the inverse matrix
        yMatrix = Matrix.MatrixInverse(yMatrix);
    }

    public static void AssignValuesToMatrices()
    {
        //count of different registry lists
        int unknownNodesCount = unknownNodes.Count;
        int shortcircuitsCount = _neighborShortcircuitRegistry.Count;
        int ledCount = _ledRegistry.Count;
        int voltageSourcesCount = _voltageSourcesRegistry.Count;

        //start row in matrix of different components
        int startRowShortcircuit  = unknownNodesCount;
        int startRowLed           = unknownNodesCount + shortcircuitsCount;
        int startRowVoltageSource = unknownNodesCount + shortcircuitsCount + ledCount;

        //handle resistors
        for (int i = 0; i < unknownNodesCount; i++)
        {
            foreach (Node neighborResistorNode in unknownNodes[i].neighborResistors)
            {
                double resistance = neighborResistorNode.nodeObject.GetComponent<Properties>().resistance;
                int indexNeighborNode = GetIndexOfNodeAfterResistor(unknownNodes[i], neighborResistorNode);

                //plus neighbor resistor at [i, i]
                yMatrix[i][i] += 1 / resistance;

                //minus neighbor resistor at [i, index of neighbor node]
                //only if the resistor is between two normal nodes (not ground)
                if (indexNeighborNode > -1)
                {
                    yMatrix[i][indexNeighborNode] -= 1 / resistance;
                }
            }
        }

        //handle shortcircuit connections
        for (int i = 0; i < shortcircuitsCount; i++)
        {
            Node[] shortCircuit = _neighborShortcircuitRegistry[i];
            int startIndex = unknownNodes.IndexOf(shortCircuit[0]);
            int endIndex = unknownNodes.IndexOf(shortCircuit[1]);

            if (!shortCircuit[0].ground && !shortCircuit[1].ground)
            {
                yMatrix[startIndex][startRowShortcircuit + i] = 1;
                yMatrix[startRowShortcircuit + i][startIndex] = 1;
                yMatrix[endIndex][startRowShortcircuit + i] = -1;
                yMatrix[startRowShortcircuit + i][endIndex] = -1;
            }

            //if one end is connected to ground
            else if (shortCircuit[0].ground)
            {
                yMatrix[endIndex][startRowShortcircuit + i] = 1;
                yMatrix[startRowShortcircuit + i][endIndex] = 1;
            }
            else if (shortCircuit[1].ground)
            {
                yMatrix[startIndex][startRowShortcircuit + i] = 1;
                yMatrix[startRowShortcircuit + i][startIndex] = 1;
            }
        }

        //handle leds
        for (int i = 0; i < ledCount; i++)
        {
            double voltage = _ledRegistry[i].nodeObject.GetComponent<Properties>().voltageDrop;
            
            //get the start/end node of the led
            Node startNode = _ledRegistry[i].nodeObject.GetComponent<Properties>().item.startObject.transform.parent.gameObject.GetComponent<Properties>().node;
            Node endNode = _ledRegistry[i].nodeObject.GetComponent<Properties>().item.endObject.transform.parent.gameObject.GetComponent<Properties>().node;

            int startIndex = unknownNodes.IndexOf(startNode);
            int endIndex = unknownNodes.IndexOf(endNode);

            if (!startNode.ground && !endNode.ground)
            {
                yMatrix[startIndex][startRowLed + i] = 1;
                yMatrix[startRowLed + i][startIndex] = 1;
                yMatrix[endIndex][startRowLed + i] = -1;
                yMatrix[startRowLed + i][endIndex] = -1;
            }

            //if one end is connected to ground
            else if (startNode.ground)
            {
                yMatrix[endIndex][startRowLed + i] = 1;
                yMatrix[startRowLed + i][endIndex] = 1;
            }
            else if (endNode.ground)
            {
                yMatrix[startIndex][startRowLed + i] = 1;
                yMatrix[startRowLed + i][startIndex] = 1;
            }

            iMatrix[startRowLed + i][0] = voltage;
        }

        //handle voltage sources
        for (int i = 0; i < voltageSourcesCount; i++)
        {
            double voltage = _voltageSourcesRegistry[i].nodeObject.GetComponent<Properties>().voltage;

            foreach (Node neighborNode in _voltageSourcesRegistry[i].neighborNodes)
            {
                if (!neighborNode.ground)
                {
                    //get the index of the node which the voltage source is connected to
                    int index = unknownNodes.IndexOf(neighborNode);

                    yMatrix[index][startRowVoltageSource + i] = 1;
                    yMatrix[startRowVoltageSource + i][index] = 1;

                    iMatrix[startRowVoltageSource + i][0] = voltage;
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

    public static void PrintMatrix(string title, double[][] matrix)
    {
        //print the given matrix
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < matrix.Length; i++)
        {
            for (int j = 0; j < matrix[0].Length; j++)
            {
                sb.Append(matrix[i][j] + "  ");
            }
            sb.AppendLine();
        }
        Debug.Log("\n" + title + "\n" + sb.ToString());
    }

    public static void PrintNodes()
    {
        Debug.Log("START");
        foreach (Node node in Node._registry)
        {
            Debug.Log(node.nodeObject.name + " " + node.nodeObject.transform.position + " ground = " + node.ground + " voltage= " + node.nodeObject.GetComponent<Properties>().voltage);
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
        _ledRegistry.Clear();
        _voltageSourcesRegistry.Clear();
        _neighborShortcircuitRegistry.Clear();
    }

    public static Node GetGroundNode()
    //get the node connected to the negative side of the battery
    {
        return groundNode;
    }
}
