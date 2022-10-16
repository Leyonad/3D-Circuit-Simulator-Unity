using System.Collections.Generic;
using UnityEngine;

public class NodeConnection
{
    public static List<NodeConnection> _registry = new();
    public readonly Node node1 = null;
    public readonly Node node2 = null;
    public readonly Item item = null;

    public static int shortcircuitAmount;
    public static int ledAmount;

    public NodeConnection(Node _node1, Node _node2, Wire _wire=null)
    {
        node1 = _node1;
        node2 = _node2;

        //get the item between the two nodes (item = null if shortcircuit)
        if (_wire != null)
        {
            item = GetItemInConnection(_wire);
            SetWireColor(_wire);
        }
        //this is the only case where _wire == null, since ground and positive are not connected with a wire
        else
            item = new Item(_node1.nodeObject.transform.parent.gameObject, "Battery");

        //dont make duplicate connections
        foreach (NodeConnection nodeConnection in _registry)
            if ((nodeConnection.node1 == _node1 && nodeConnection.node2 == _node2 && nodeConnection.item == item)
                || (nodeConnection.node2 == _node1 && nodeConnection.node1 == _node2 && nodeConnection.item == item))
                return;

        if (item != null)
        {
            if (item.type == "LED")
                ledAmount += 1;
            Debug.Log($"NEW CONNECTION:   {_node1.nodeObject.name}   -->   {item.itemObject.name}   -->   {_node2.nodeObject.name}");
        }
        else
        {
            shortcircuitAmount += 1;
            Debug.Log($"NEW CONNECTION:   {_node1.nodeObject.name}   -->   {_node2.nodeObject.name}");
        }
        _registry.Add(this);
    }

    private Item GetItemInConnection(Wire wire)
    {
        //return the item which is between the two nodes, null if there is no item
        return WireManager.GetItemAttachedToWire(wire);
    }

    private void SetWireColor(Wire wire)
    {
        //this method changes the color of the wire depending on the type of connection
        if (WireManager.electricityPathView)
            return;

        if (item != null)
            return;

        //shortcircuit to positive
        if (NodeManager.IsPositive(node1) || NodeManager.IsPositive(node2))
        {
            wire.wireColor = ResourcesManager.wireMaterial;
            Debug.Log("POSIRIVR");
        }
        
        //shortcircuit to ground
        else if (NodeManager.IsGround(node1) || NodeManager.IsGround(node2))
            wire.wireColor = ResourcesManager.black;
        
        //normal shortcircuit
        else
            wire.wireColor = ResourcesManager.wireShortcircuitMaterial;

        wire.lineRenderer.material = wire.wireColor;
    }
}
