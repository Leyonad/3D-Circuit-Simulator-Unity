using UnityEngine;

public class Battery
{
    public GameObject batteryObject;

    public Battery(Vector3 batteryPosition, string type)
    {
        if (type == "9V")
        {
            batteryObject = Object.Instantiate(ResourcesManager.prefabBattery9V, batteryPosition, Quaternion.identity);
        }
        else Debug.Log("Type of Battery not found!");
        if (batteryObject != null)
        {
            batteryObject.transform.SetParent(ComponentsManager.components.transform);
        }
    }
}
