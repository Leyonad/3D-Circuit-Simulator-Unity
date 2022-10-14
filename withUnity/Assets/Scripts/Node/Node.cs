using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public static List<Node> _registry = new();
    public GameObject nodeObject;

    public Node(GameObject _obj)
    {
        //dont make duplicate nodes
        foreach (Node node in _registry)
            if (node.nodeObject == _obj)
                return;

        nodeObject = _obj;
        _registry.Add(this);
        _obj.GetComponent<Properties>().node = this;
    }

    public List<Wire> GetAttachedWires()
    {
        return nodeObject.GetComponent<Properties>().attachedWires;
    }
}
