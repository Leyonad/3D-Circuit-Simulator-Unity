using UnityEngine;

public class LED
{
    public GameObject LEDObject;
    private readonly float defaultYValue = 5f;
    public static LED justCreated = null;
    public Wire wire1;
    public Wire wire2;

    private GameObject m1obj;
    private GameObject m2obj;

    public LED(GameObject collideObject)
    {
        Vector3 spawnPosition = collideObject.transform.position;
        spawnPosition.y = defaultYValue;
        LEDObject = Object.Instantiate(ResourcesManager.prefabLED, spawnPosition, Quaternion.identity);
        LEDObject.transform.SetParent(ComponentsManager.components.transform);

        m1obj = LEDObject.transform.Find("m1").gameObject;
        m2obj = LEDObject.transform.Find("m2").gameObject;

        wire1 = new Wire(collideObject, m1obj);
        wire2 = new Wire(m2obj);

        wire1.lineRenderer.material = ResourcesManager.grey;
        wire2.lineRenderer.material = ResourcesManager.grey;

        justCreated = this;
    }

    public void Move()
    {
        //position = (A+B)/2
        //rotation
        float angle = Functions.GetYRotationBetween2Points(wire1.startObject.transform.position, wire2.lineRenderer.GetPosition(wire2.verticesAmount - 1));
        Vector3 targetPosition = ((wire2.lineRenderer.GetPosition(wire2.verticesAmount - 1) + wire1.startObject.transform.position)) / 2;
        LEDObject.transform.position = new Vector3(targetPosition.x, defaultYValue, targetPosition.z);
        LEDObject.transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);

        wire1.lineRenderer.SetPosition(wire1.verticesAmount - 1, m1obj.transform.position);
        wire2.lineRenderer.SetPosition(0, m2obj.transform.position);
    }
}
