using System.Collections;
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

    public static void CalculateNodes()
    {
        PrintNodes();
        MakeConnectionsBetweenNodes();
        
    }

    private static void MakeConnectionsBetweenNodes()
    {
        Node groundNode = null;
        Node positiveNode = null;
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
        new NodeConnection(positiveNode, groundNode);
    }

    private static void AssignResultVoltagesToNodes()
    {
        for (int i = 0; i < unknownNodes.Count; i++)
        {
            unknownNodes[i].nodeObject.GetComponent<Properties>().voltage = resultMatrix[i][0];
        }
    }

    private static void CreateMatrices()
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

    private static void CalculateResultMatrix()
    {
        resultMatrix = Matrix.MatrixProduct(yMatrix, iMatrix);
    }

    private static void CalculateInverseMatrix()
    {
        //calculate the inverse matrix
        yMatrix = Matrix.MatrixInverse(yMatrix);
    }

    private static void AssignValuesToMatrices()
    {
        //count of different registry lists

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

    public static void ClearAllNodes()
    {
        Node._registry.Clear();
    }
}
