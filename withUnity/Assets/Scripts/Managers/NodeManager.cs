using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class NodeManager
{
    //matrix stuff
    private static List<Node> unknownNodes = new List<Node>();
    private static int matrixDimension;
    private static double[][] yMatrix;
    private static double[][] iMatrix;
    private static double[][] resultMatrix;

    //ground and positive node
    private static Node groundNode = null;
    private static Node positiveNode = null;

    public static void CalculateNodes()
    {
        //if the connections can be made, move on with the next steps
        if (MakeConnectionsBetweenNodes())
        {
            CreateMatrices();
            AssignValuesToMatrices();
            //PrintMatrix("yMatrix", yMatrix);
            //PrintMatrix("iMatrix", iMatrix);
            CalculateInverseMatrix();
            CalculateResultMatrix();
            //PrintMatrix("Result", resultMatrix);

            //if the result voltages are not infinite, the current can be calculated
            if (AssignResultVoltagesToNodes())
            {
                AssignCurrentsToItems();
            }
        }
    }

    private static bool MakeConnectionsBetweenNodes()
    {
        //this method creates connections between nodes
        foreach (Node node in Node._registry)
        {
            foreach (Wire wire in node.GetAttachedWires())
            {
                Node node2 = WireManager.GetNodeAfterWire(node, wire);
                new NodeConnection(node, node2, wire);
            }

            //find positive and ground node
            if (groundNode == null && node.nodeObject.name == "Ground")
                groundNode = node; 
            else if (positiveNode == null && node.nodeObject.name == "Positive") 
                positiveNode = node;
        }

        //create node between positive and ground
        if (positiveNode != null && groundNode != null)
        {
            new NodeConnection(positiveNode, groundNode);
            return true;
        }
        //if one of them is null, it means that the circuit is not complete
        else
        {
            Debug.Log("CIRCUIT NOT COMPLETE");
            return false;
        }
    }

    private static void CreateMatrices()
    {
        //-1 for ground, +1 for battery
        matrixDimension = Node._registry.Count - 1 + NodeConnection.shortcircuitAmount + NodeConnection.ledAmount + 1;
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

    private static void CalculateResultMatrix()
    {
        resultMatrix = Matrix.MatrixProduct(yMatrix, iMatrix);
    }

    private static bool AssignResultVoltagesToNodes()
    {
        for (int i = 0; i < unknownNodes.Count; i++)
        {
            if (Double.IsNaN(resultMatrix[i][0]))
            {
                Debug.Log("SINGULAR MATRIX: CIRCUIT CANNOT BE CALCULATED");
                return false;
            }
            unknownNodes[i].nodeObject.GetComponent<Properties>().voltage = resultMatrix[i][0];
        }
        return true;
    }

    public static void AssignCurrentsToItems()
    {
        int ledSeen = 0;

        //this method assigns currents to items in connections
        foreach (NodeConnection nC in NodeConnection._registry)
        {
            if (nC.item != null)
            {
                //if the item is a resistor
                if (nC.item.type == "Resistor")
                {
                    double resistance = nC.item.itemObject.GetComponent<Properties>().resistance;
                    double voltageNode1 = nC.node1.nodeObject.GetComponent<Properties>().voltage;
                    double voltageNode2 = nC.node2.nodeObject.GetComponent<Properties>().voltage;

                    double current = (voltageNode1 - voltageNode2) / resistance;
                    nC.item.itemObject.GetComponent<Properties>().current = Math.Abs(current);
                }

                // if the item is an led
                else if (nC.item.type == "LED")
                {
                    int indexInResultMatrix = unknownNodes.Count + NodeConnection.shortcircuitAmount;
                    double current = resultMatrix[indexInResultMatrix + ledSeen][0];
                    nC.item.itemObject.GetComponent<Properties>().current = Math.Abs(current);
                    UpdateGlowIntensity(nC.item.itemObject, current);
                    ledSeen += 1;
                }
            }
        }
    }

    public static void UpdateGlowIntensity(GameObject itemObject, double _current)
    {
        //make a LED glow if not too less and not too much current flows through it
        float current = (float) _current;
        float minCurrent = (float) itemObject.GetComponent<Properties>().minCurrent;
        float maxCurrent = (float) itemObject.GetComponent<Properties>().maxCurrent;

        float minGlowIntensity = 1f;
        float maxGlowIntensity = 5f;

        float glowIntensity = Functions.MapToRange(current, minCurrent, maxCurrent, minGlowIntensity, maxGlowIntensity);

        //get the current emissionColor
        Vector4 currentEmissionColor = itemObject.GetComponent<Properties>().item.
                    itemObject.GetComponent<MeshRenderer>().material.GetColor("_EmissionColor");

        float newIntensity;
        //if there is too less or too much current the led doesnt glow
        if (current < minCurrent)
        {
            Debug.Log("Too less current is flowing through an LED!");
            newIntensity = 0f;
        }
        else if (current > maxCurrent)
        {
            Debug.Log("Too much current is flowing through an LED!");
            newIntensity = 0f;
        }
        else
            newIntensity = glowIntensity;

        //get the new emissionColor
        Color emissionColor = Functions.ChangeHDRColorIntensity(currentEmissionColor, newIntensity);

        //set the new emissionColor
        itemObject.GetComponent<Properties>().item.
                    itemObject.GetComponent<MeshRenderer>().material.
                    SetColor("_EmissionColor", emissionColor);
    }

    private static void CalculateInverseMatrix()
    {
        //calculate the inverse matrix
        yMatrix = Matrix.MatrixInverse(yMatrix);
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
            Debug.Log(node.nodeObject.name + " " + node.nodeObject.transform.position);
        }
        Debug.Log("END");
    }

    private static bool IsGround(Node node)
    {
        return node == groundNode;
    }

    public static void ClearAllNodes()
    {
        Node._registry.Clear();
        NodeConnection._registry.Clear();
        NodeConnection.shortcircuitAmount = 0;
        NodeConnection.ledAmount = 0;
        groundNode = null;
        positiveNode = null;

    }

    private static void AssignValuesToMatrices()
    {
        unknownNodes = Node._registry;
        unknownNodes.Remove(groundNode);

        int shortcircuitRow = unknownNodes.Count;
        int ledRow = shortcircuitRow + NodeConnection.shortcircuitAmount;
        int voltagesourceRow = ledRow + NodeConnection.ledAmount;

        int shortcircuitSeen = 0;
        int ledSeen = 0;
        int voltagesourceSeen = 0;

        foreach (NodeConnection nC in NodeConnection._registry)
        {
            int indexNode1 = unknownNodes.IndexOf(nC.node1);
            int indexNode2 = unknownNodes.IndexOf(nC.node2);

            //if one node of the connection is ground
            if (IsGround(nC.node1) || IsGround(nC.node2))
            {
                //find out which one of the nodes is the metalstrip
                Node metalStripNode;
                int metalStripIndex;
                if (indexNode1 == -1)
                {
                    metalStripNode = nC.node2;
                    metalStripIndex = indexNode2;
                }
                else
                {
                    metalStripNode = nC.node1;
                    metalStripIndex = indexNode1;
                }

                //if there is no item in the connection (meaning a shortcircuit from ground to a node)
                if (nC.item == null)
                {
                    yMatrix[metalStripIndex][shortcircuitRow + shortcircuitSeen] = 1;
                    yMatrix[shortcircuitRow + shortcircuitSeen][metalStripIndex] = 1;
                    shortcircuitSeen += 1;
                }

                //if there is an led in the connection
                else if (nC.item.type == "LED")
                {
                    //check if the connection is in the right direction because of polarity of the LED
                    if (WireManager.GetWireBetweenTwoGameObjects(metalStripNode.nodeObject, nC.item.itemObject) == nC.item.wire1)
                    {
                        yMatrix[metalStripIndex][ledRow + ledSeen] = 1;
                        yMatrix[ledRow + ledSeen][metalStripIndex] = 1;
                    }
                    else
                    {
                        yMatrix[metalStripIndex][ledRow + ledSeen] = -1;
                        yMatrix[ledRow + ledSeen][metalStripIndex] = -1;
                    }
                    iMatrix[ledRow + ledSeen][0] = nC.item.itemObject.GetComponent<Properties>().voltageDrop;
                    ledSeen += 1;
                }

                //if there is a resistor in the connection
                else if (nC.item.type == "Resistor")
                {
                    yMatrix[metalStripIndex][metalStripIndex] += 1 / nC.item.itemObject.GetComponent<Properties>().resistance;
                }

                //if there is a voltage source in the connection
                else if (nC.item.type == "Battery")
                {
                    yMatrix[metalStripIndex][voltagesourceRow + voltagesourceSeen] = 1;
                    yMatrix[voltagesourceRow + voltagesourceSeen][metalStripIndex] = 1;
                    iMatrix[voltagesourceRow + voltagesourceSeen][0] = nC.item.itemObject.GetComponent<Properties>().voltage;
                    voltagesourceSeen += 1;
                }
            }

            //if no node of the connection is ground
            else
            {
                //if there is no item in the connection
                if (nC.item == null)
                {
                    yMatrix[indexNode1][shortcircuitRow + shortcircuitSeen] = 1;
                    yMatrix[shortcircuitRow + shortcircuitSeen][indexNode1] = 1;
                    yMatrix[indexNode2][shortcircuitRow + shortcircuitSeen] = -1;
                    yMatrix[shortcircuitRow + shortcircuitSeen][indexNode2] = -1;
                    shortcircuitSeen += 1;
                }

                //if there is an led in the connection
                else if (nC.item.type == "LED")
                {
                    //check if the connection is in the right direction because of polarity of the LED
                    if (WireManager.GetWireBetweenTwoGameObjects(nC.node1.nodeObject, nC.item.itemObject) == nC.item.wire1)
                    {
                        yMatrix[indexNode1][ledRow + ledSeen] = 1;
                        yMatrix[ledRow + ledSeen][indexNode1] = 1;
                        yMatrix[indexNode2][ledRow + ledSeen] = -1;
                        yMatrix[ledRow + ledSeen][indexNode2] = -1;
                    }
                    else
                    {
                        yMatrix[indexNode1][ledRow + ledSeen] = -1;
                        yMatrix[ledRow + ledSeen][indexNode1] = -1;
                        yMatrix[indexNode2][ledRow + ledSeen] = 1;
                        yMatrix[ledRow + ledSeen][indexNode2] = 1;
                    }
                    iMatrix[ledRow + ledSeen][0] = nC.item.itemObject.GetComponent<Properties>().voltageDrop;
                    ledSeen += 1;
                }

                //if there is a resistor in the connection
                else if (nC.item.type == "Resistor")
                {
                    double resistance = nC.item.itemObject.GetComponent<Properties>().resistance;
                    yMatrix[indexNode1][indexNode1] += 1 / resistance;
                    yMatrix[indexNode2][indexNode2] += 1 / resistance;
                    yMatrix[indexNode1][indexNode2] -= 1 / resistance;
                    yMatrix[indexNode2][indexNode1] -= 1 / resistance;
                }

                //case with battery is not necessary as that case will never occur
                //if there is a battery in the connection
                else if (nC.item.type == "Battery")
                {
                    yMatrix[indexNode1][voltagesourceRow + voltagesourceSeen] = 1;
                    yMatrix[voltagesourceRow + voltagesourceSeen][indexNode1] = 1;
                    yMatrix[indexNode2][voltagesourceRow + voltagesourceSeen] = -1;
                    yMatrix[voltagesourceRow + voltagesourceSeen][indexNode2] = -1;
                    iMatrix[voltagesourceRow + voltagesourceSeen][0] = nC.item.itemObject.GetComponent<Properties>().voltage;
                    voltagesourceSeen += 1;
                }
            }
        }
    }
}
