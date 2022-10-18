using UnityEngine;

public class Battery
{
    public GameObject batteryObject;

    public Battery(Vector3 batteryPosition, string type)
    {
        if (type == "9V")
        {
            batteryObject = Object.Instantiate(ResourcesManager.prefabBattery9V, batteryPosition, Quaternion.identity);
            batteryObject.transform.rotation = Quaternion.Euler(new Vector3(0, 90f, -90f));
        }
        else if (type == "V2")
        {
            batteryObject = Object.Instantiate(ResourcesManager.prefabBatteryV2, batteryPosition, Quaternion.identity);
            batteryObject.transform.rotation = Quaternion.Euler(new Vector3(-90f, 90f, 0));
        }
        else Debug.Log("Type of Battery not found!");
        if (batteryObject != null)
        {
            batteryObject.transform.SetParent(ComponentsManager.components.transform);
            batteryObject.name = "Battery";
        }
    }
}
