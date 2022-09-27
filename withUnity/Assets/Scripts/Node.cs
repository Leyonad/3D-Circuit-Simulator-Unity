using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public static List<Node> _registry = new List<Node>();
    public static bool foundGround = false;
    
    public GameObject nodeObject;
    private float voltage;
    private bool ground;

    public Node(GameObject _obj, bool _ground=false)
    {
        nodeObject = _obj;
        ground = _ground;

        _registry.Add(this);
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

    public static void ClearAllNodes()
    {
        _registry.Clear();
    }

    public void UpdateVoltageOfNode(float _voltage)
    {
        voltage = _voltage;
    }
}
