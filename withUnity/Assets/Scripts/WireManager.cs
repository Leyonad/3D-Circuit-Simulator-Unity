using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class WireManager : MonoBehaviour
{
    [SerializeField]
    GameObject debugObject;
    [SerializeField] 
    public static Camera cam;
    public static Material wireMaterial;
    public int numCapVertices = 4;

    public static GameObject breadboard;

    private void Start()
    {
        breadboard = GameObject.FindGameObjectWithTag("Breadboard");
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        wireMaterial = Resources.Load("Materials/Wire_Material", typeof(Material)) as Material;
        if (wireMaterial == null)
            Debug.Log("Wire Material not found!");
    }

    private void Update()
    {
        //DEBUG PRINT VERTICES-------------------------------------------
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            //foreach (Wire wire in Wire._registry)
            //    foreach (Vector3 vertice in wire.verticesOfWire)
            //       print(vertice);
            //Debug.Log(debugObject.transform.localPosition);
        }
        //DEBUG PRINT VERTICES-------------------------------------------

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
                    if (!hit.collider.gameObject.CompareTag("Untagged"))
                        if (hit.collider.gameObject.transform.parent != null)
                            if (hit.collider.gameObject.transform.parent.CompareTag("Metals"))
                                wirePossible = true;
                
                if (wirePossible)
                {
                    //Check if the wire doesnt already exist
                    Wire.justCreated.endObject = hit.collider.gameObject;
                    if (!WireAlreadyExists(Wire.justCreated))
                    {
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
    }


    public class Wire
    {
        public static List<Wire> _registry = new List<Wire>();

        public int verticesAmount = 2;
        public GameObject startObject;
        public GameObject endObject;
        public GameObject lineObject;
        public LineRenderer lineRenderer;
        public static Wire justCreated;

        public Wire(GameObject collideObject)
        {
            //Later do this in a loop
            //start and end positions must be the same in the beginning
            //end position changes later to mouse position
            startObject = collideObject;
            if (!WireAlreadyExists(this))
            {
                CreateLineObject();
                justCreated = this;
            }
        }

        public void WireFollowMouse(Wire justCreated)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector3 targetPosition = MouseInteraction.GetNewPosition(cam, mousePosition, Vector2.zero, justCreated.lineRenderer.GetPosition(justCreated.verticesAmount-1));
            
            justCreated.lineRenderer.SetPosition(verticesAmount - 1, targetPosition);
        }

        private void CreateLineObject()
        {
            lineObject = new GameObject($"wire{_registry.Count + 1} [{startObject}]");
            lineRenderer = lineObject.AddComponent<LineRenderer>();
            lineRenderer.material = wireMaterial;
            lineRenderer.widthMultiplier = 0.1f;
            lineRenderer.numCapVertices = 4;
            lineRenderer.SetPosition(0, startObject.transform.position);
            lineRenderer.SetPosition(verticesAmount - 1, startObject.transform.position);

            UpdateLinesOfWire();
        }

        public void UpdateLinesOfWire()
        {
            
        }
    }

    /*
    public static bool IsPointWithinCollider(Collider collider, Vector3 point)
    {
        return collider.ClosestPoint(point) == point;
    }
    */

    public static bool WireAlreadyExists(Wire wire)
    {
        foreach (Wire existingWire in Wire._registry)
        {
            if (wire.startObject == existingWire.startObject
                || wire.startObject == existingWire.endObject
                || wire.endObject == existingWire.startObject
                || wire.endObject == existingWire.endObject)
            {
                Debug.Log("WIRE ALREADY EXISTS!");
                return true;
            }
        }
        return false;
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
