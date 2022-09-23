using System.Collections.Generic;
using UnityEngine;

public class LED
{
    public GameObject itemObject;
    private static readonly float defaultYValue = 3f;
    private float currentYPosition;
    public static LED justCreated = null;
    public Wire wire1;
    public Wire wire2;

    private GameObject m1obj;
    private GameObject m2obj;

    public GameObject startObject;
    public GameObject endObject;

    public static LED selectedLED = null;

    public static List<LED> _registry = new List<LED>();

    public LED(GameObject collideObject)
    {
        Vector3 spawnPosition = collideObject.transform.position;
        currentYPosition = defaultYValue;
        spawnPosition.y = defaultYValue;
        itemObject = Object.Instantiate(ResourcesManager.prefabLED, spawnPosition, Quaternion.identity);
        itemObject.transform.SetParent(ComponentsManager.components.transform);

        m1obj = itemObject.transform.Find("m1").gameObject;
        m2obj = itemObject.transform.Find("m2").gameObject;

        wire1 = new Wire(collideObject, m1obj, 1.5f);
        wire2 = new Wire(m2obj, null, 1.5f);

        startObject = collideObject;

        wire1.lineRenderer.material = ResourcesManager.grey;
        wire2.lineRenderer.material = ResourcesManager.grey;

        justCreated = this;

        _registry.Add(this);
    }

    public void Move()
    {
        UpdatePositionAndRotation();

        wire1.lineRenderer.SetPosition(wire1.verticesAmount - 1, m1obj.transform.position);
        wire1.UpdateLinesOfWire();

        wire2.lineRenderer.SetPosition(0, m2obj.transform.position);
        wire2.UpdateLinesOfWire();
    }

    public void UpdateYPosition(Vector3 targetPosition)
    {
        currentYPosition = targetPosition.y;
        LED.selectedLED.itemObject.transform.position = targetPosition;
        wire1.lineRenderer.SetPosition(wire1.verticesAmount - 1, m1obj.transform.position);
        wire2.lineRenderer.SetPosition(0, m2obj.transform.position);
        wire1.UpdateLinesOfWire();
        wire2.UpdateLinesOfWire();
    }


    public void UpdatePositionAndRotation()
    {
        //position = (A+B)/2
        Vector3 targetPosition = ((wire2.lineRenderer.GetPosition(wire2.verticesAmount - 1) + startObject.transform.position)) / 2;
        itemObject.transform.position = new Vector3(targetPosition.x, currentYPosition, targetPosition.z);
        
        //rotation = angle between p1 and p2
        float angle = Functions.GetYRotationBetween2Points(startObject.transform.position, wire2.lineRenderer.GetPosition(wire2.verticesAmount - 1));
        itemObject.transform.rotation = Quaternion.AngleAxis(-angle, Vector3.up);
    }

    public static void UpdatePositionsAndRotations(GameObject obj)
    {
        foreach (LED led in LED._registry)
            if (led.startObject.transform.IsChildOf(obj.transform) || led.endObject.transform.IsChildOf(obj.transform))
                led.Move();
    }
}
