using System.Collections.Generic;
using UnityEngine;

public class Item
{
    public GameObject itemObject;
    private static readonly float defaultYValue = 3f;
    private float currentYPosition;
    public static Item justCreated = null;
    public static bool moveItemUpDown = false;

    public static float minItemY = 2f;
    public static float maxItemY = 6f;

    public Material wireColor;
    public float wireThickness;
    public Wire wire1;
    public Wire wire2;

    private GameObject m1obj;
    private GameObject m2obj;

    public GameObject startObject;
    public GameObject endObject;

    public static Item selectedItem = null;

    public Material ledColor;

    public List<Wire> wiresOfItem = new List<Wire>();

    public static List<Item> _registry = new List<Item>();

    public Item(GameObject collideObject, string type, string color="red")
    {
        startObject = collideObject;
        Vector3 spawnPosition = startObject.transform.position;
        currentYPosition = defaultYValue;
        spawnPosition.y = defaultYValue;

        if (type == "LED")
        {
            itemObject = Object.Instantiate(ResourcesManager.prefabLED, spawnPosition, Quaternion.identity);
            ledColor = ResourcesManager.LED_red;
            itemObject.GetComponent<Properties>().voltageDrop = 2f;
            if (color == "green")
            {
                ledColor = ResourcesManager.LED_green;
                itemObject.GetComponent<Properties>().voltageDrop = 3f;
            }
            else if (color == "yellow")
            {
                ledColor = ResourcesManager.LED_yellow;
                itemObject.GetComponent<Properties>().voltageDrop = 2.3f;
            }
            else if (color == "blue")
            {
                ledColor = ResourcesManager.LED_blue;
                itemObject.GetComponent<Properties>().voltageDrop = 3.4f;
            }
            wireColor = ResourcesManager.grey;
            wireThickness = 0.05f;
        }
        else if (type == "Resistor")
        {
            itemObject = Object.Instantiate(ResourcesManager.prefabResistor, spawnPosition, Quaternion.identity);
            itemObject.GetComponent<Properties>().resistance = 1000f;
            itemObject.GetComponent<Properties>().tolerance = 0.05f;
            wireColor = ResourcesManager.grey;
            wireThickness = 0.05f;
        }
        
        itemObject.name = type;
        itemObject.GetComponent<Properties>().item = this;
        itemObject.transform.SetParent(ComponentsManager.components.transform);

        m1obj = itemObject.transform.Find("m0").gameObject;
        m2obj = itemObject.transform.Find("m1").gameObject;

        wire1 = new Wire(startObject, m1obj, 1.5f, this);
        wire2 = new Wire(m2obj, null, 1.5f, this);

        //set poles for wires, where the first wire is negative and the last positive
        wire1.lineObject.GetComponent<Properties>().pole = 1;
        wire2.lineObject.GetComponent<Properties>().pole = 0;

        wiresOfItem.Add(wire1);
        wiresOfItem.Add(wire2);

        wire1.lineRenderer.material = wireColor;
        wire2.lineRenderer.material = wireColor;

        justCreated = this;
    }

    public static void UpdateItemAll(GameObject obj)
    {
        foreach (Item item in Item._registry)
            if (item.startObject.transform.IsChildOf(obj.transform) || item.endObject.transform.IsChildOf(obj.transform))
                item.UpdateItem();
    }

    public void UpdateItem()
    {
        UpdateItemPositionAndRotation();

        wire1.lineRenderer.SetPosition(wire1.verticesAmount - 1, m1obj.transform.position);
        wire2.lineRenderer.SetPosition(0, m2obj.transform.position);
        
        wire1.UpdatePointsOfWire();
        wire2.UpdatePointsOfWire();
    }

    public void UpdateItemPositionAndRotation()
    {
        //position = (A+B)/2
        Vector3 targetPosition = ((wire2.lineRenderer.GetPosition(wire2.verticesAmount - 1) + startObject.transform.position)) / 2;
        itemObject.transform.position = new Vector3(targetPosition.x, currentYPosition, targetPosition.z);
        
        //rotation = angle between p1 and p2
        float angle = Functions.GetYRotationBetween2Points(startObject.transform.position, wire2.lineRenderer.GetPosition(wire2.verticesAmount - 1));
        itemObject.transform.rotation = Quaternion.AngleAxis(-angle, Vector3.up);
    }

    public void UpdateYPosition(Vector3 targetPosition)
    {
        currentYPosition = targetPosition.y;
        Item.selectedItem.itemObject.transform.position = targetPosition;
        wire1.lineRenderer.SetPosition(wire1.verticesAmount - 1, m1obj.transform.position);
        wire2.lineRenderer.SetPosition(0, m2obj.transform.position);
        wire1.UpdatePointsOfWire();
        wire2.UpdatePointsOfWire();
    }

    public static void Unselect()
    {
        Item.selectedItem = null;
        Item.moveItemUpDown = false;
    }
}
