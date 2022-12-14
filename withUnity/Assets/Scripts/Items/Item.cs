using System.Collections.Generic;
using UnityEngine;

public class Item
{
    public GameObject itemObject;
    public string type;
    public static Item justCreated = null;
    public static bool moveItemUpDown = false;

    private float currentYPosition;
    private readonly float defaultYValue;
    public static float minItemY;
    public static float maxItemY;

    public Material wireColor;
    public float wireThickness;
    public Wire wire1;
    public Wire wire2;

    private GameObject m1obj;
    private GameObject m2obj;

    public GameObject startObject;
    public GameObject endObject;

    public Material itemMaterial;

    public List<Wire> wiresOfItem = new List<Wire>();

    public static List<Item> _registry = new List<Item>();

    public Item(GameObject collideObject, string _type, string color="red")
    {
        startObject = collideObject;
        type = _type;

        if (_type == "Battery")
        {
            itemObject = collideObject;
            return;
        }

        Vector3 spawnPosition = startObject.transform.position;

        if (_type == "LED")
        {
            defaultYValue = 3f;
            minItemY = 1.5f;
            maxItemY = 5f;
            spawnPosition.y = defaultYValue;
            itemObject = Object.Instantiate(ResourcesManager.prefabLED, spawnPosition, Quaternion.identity);
            itemMaterial = ResourcesManager.LED_red;
            itemObject.GetComponent<Properties>().voltageDrop = 1.9;
            if (color == "green")
            {
                itemMaterial = ResourcesManager.LED_green;
                itemObject.GetComponent<Properties>().voltageDrop = 3;
            }
            else if (color == "yellow")
            {
                itemMaterial = ResourcesManager.LED_yellow;
                itemObject.GetComponent<Properties>().voltageDrop = 2.3;
            }
            else if (color == "blue")
            {
                itemMaterial = ResourcesManager.LED_blue;
                itemObject.GetComponent<Properties>().voltageDrop = 3.4;
            }
            wireColor = ResourcesManager.grey;
            wireThickness = 0.05f;
            itemObject.GetComponent<MeshRenderer>().material = itemMaterial;
            itemObject.GetComponent<Properties>().requiredCurrent = 20;
            itemObject.GetComponent<Properties>().minCurrent = 2;
            itemObject.GetComponent<Properties>().maxCurrent = 30;
        }
        else if (_type == "Resistor")
        {
            defaultYValue = 1.5f;
            minItemY = 1.3f;
            maxItemY = 5f;
            spawnPosition.y = defaultYValue;
            itemObject = Object.Instantiate(ResourcesManager.prefabResistor, spawnPosition, Quaternion.identity);
            wireColor = ResourcesManager.grey;
            wireThickness = 0.05f;
            itemObject.GetComponent<Properties>().resistance = 0.5;
        }

        currentYPosition = defaultYValue;

        itemObject.name = _type;
        itemObject.GetComponent<Properties>().item = this;
        itemObject.transform.SetParent(ComponentsManager.components.transform);

        m1obj = itemObject.transform.Find("m0").gameObject;
        m2obj = itemObject.transform.Find("m1").gameObject;

        wire1 = new Wire(startObject, m1obj, 1.5f, this);
        wire2 = new Wire(m2obj, null, 1.5f, this);

        if (_type == "LED")
        {
            //set poles for wires, where the first wire is positive and the last one negative
            wire1.lineObject.GetComponent<Properties>().polarity = 0;
            wire2.lineObject.GetComponent<Properties>().polarity = 1;
        }

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
        itemObject.transform.position = targetPosition;
        wire1.lineRenderer.SetPosition(wire1.verticesAmount - 1, m1obj.transform.position);
        wire2.lineRenderer.SetPosition(0, m2obj.transform.position);
        wire1.UpdatePointsOfWire();
        wire2.UpdatePointsOfWire();
    }
}
