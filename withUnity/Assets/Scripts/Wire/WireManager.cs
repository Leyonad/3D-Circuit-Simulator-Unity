using UnityEngine;
using UnityEngine.InputSystem;

public class WireManager : MonoBehaviour
{
    [SerializeField]
    GameObject debugObject;
    [SerializeField] 
    public static Camera cam;
    public int numCapVertices = 4;

    public static Wire selectedWire;

    private void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    private void Update()
    {
        if (Wire.justCreated != null)
        {
            Wire.justCreated.WireFollowMouse(Wire.justCreated);

            //cancel creation of a new wire by destroying it
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                Destroy(Wire.justCreated.lineObject);
                Wire.justCreated = null;
            }
            //finish creation of a new wire by adding it to _registry
            else if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                //check if clicked on a gameobject
                RaycastHit hit = CastRay();

                bool wirePossible = false;
                if (hit.collider != null)
                    if (isMetal(hit.collider.gameObject))
                        wirePossible = true;
                
                if (wirePossible)
                {
                    //Check if the wire doesnt already exist
                    Wire.justCreated.endObject = hit.collider.gameObject;
                    if (Wire.justCreated.endObject != Wire.justCreated.startObject && WireAlreadyExists(Wire.justCreated.endObject) == null)
                    {
                        //current metalstrip endObject = current metalstrip startObject
                        if (hit.collider.gameObject.transform.parent.CompareTag("MetalStrip")) {
                            float startCurrent;
                            if (Wire.justCreated.startObject.transform.parent.CompareTag("MetalStrip"))
                                startCurrent = Wire.justCreated.startObject.transform.parent.GetComponent<MetalStripProperties>().current;
                            else
                                startCurrent = Wire.justCreated.startObject.GetComponent<BatteryProperties>().current;
                            hit.collider.gameObject.transform.parent.GetComponent<MetalStripProperties>().current = startCurrent;
                        }

                        Wire.justCreated.lineRenderer.SetPosition(Wire.justCreated.verticesAmount - 1, hit.collider.gameObject.transform.position);
                        Wire.justCreated.UpdateLinesOfWire();
                        Wire._registry.Add(Wire.justCreated);
                    }
                    else Destroy(Wire.justCreated.lineObject);
                }
                else Destroy(Wire.justCreated.lineObject);
                
                //always set to null if mouse pressed
                Wire.justCreated = null;
            }
        }

        else if (Keyboard.current.deleteKey.wasReleasedThisFrame)
        {
            if (selectedWire != null)
            {
                Wire._registry.Remove(selectedWire);
                Destroy(selectedWire.lineObject);
                selectedWire = null;
            }
        }
    }

    public static void SelectWire(Wire wire)
    {
        selectedWire = wire;
        selectedWire.lineRenderer.material = ResourcesManager.highlightWireMaterial;
    }

    public static void UnselectWire()
    {
        if (selectedWire == null)
            return;
        selectedWire.lineRenderer.material = ResourcesManager.wireMaterial;
        selectedWire = null;
    }

    public static bool isMetal(GameObject obj)
    {
        if (obj.CompareTag("Metal"))
            return true;
        if (obj.transform.parent != null)
            if (obj.transform.parent.gameObject.CompareTag("Metals"))
                return true;
        return false;
    }

    public static Wire WireAlreadyExists(GameObject wireToObject)
    {
        foreach (Wire existingWire in Wire._registry)
        {
            if (wireToObject == existingWire.startObject
                || wireToObject == existingWire.endObject
                || wireToObject == existingWire.startObject
                || wireToObject == existingWire.endObject)
            {
                //Debug.Log("WIRE ALREADY EXISTS!");
                return existingWire;
            }
        }
        return null;
    }

    public Vector3 RoundedVector(Vector3 vec)
    {
        vec *= 10f;
        vec = new Vector3(Mathf.Round(vec.x), Mathf.Round(vec.y), Mathf.Round(vec.z));
        vec /= 10f;

        return vec;
    }

    private RaycastHit CastRay()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 screenMousePosFar = new Vector3(
            mousePosition.x,
            mousePosition.y,
            cam.farClipPlane
        );

        Vector3 screenMousePosNear = new Vector3(
            mousePosition.x,
            mousePosition.y,
            cam.nearClipPlane
        );

        Vector3 worldMousePosFar = cam.ScreenToWorldPoint(screenMousePosFar);
        Vector3 worldMousePosNear = cam.ScreenToWorldPoint(screenMousePosNear);

        RaycastHit hit;
        Physics.Raycast(worldMousePosNear, worldMousePosFar - worldMousePosNear, out hit);

        return hit;
    }

    

}
