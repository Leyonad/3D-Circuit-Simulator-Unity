using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WireManager : MonoBehaviour
{
    [SerializeField]
    GameObject debugObject;
    [SerializeField] 
    public static Camera cam;
    public static Material wireMaterial;
    public static Material highlightWireMaterial;
    public int numCapVertices = 4;

    public static Wire selectedWire;

    private void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        wireMaterial = LoadMaterial("Wire_Material");
        highlightWireMaterial = LoadMaterial("Highlight_Wire_Material");
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
                    if (!hit.collider.gameObject.CompareTag("Untagged"))
                        if (hit.collider.gameObject.transform.parent != null)
                            if (hit.collider.gameObject.transform.parent.CompareTag("Metals"))
                                wirePossible = true;
                
                if (wirePossible)
                {
                    //Check if the wire doesnt already exist
                    Wire.justCreated.endObject = hit.collider.gameObject;
                    if (Wire.justCreated.endObject != Wire.justCreated.startObject && WireAlreadyExists(Wire.justCreated.endObject) == null)
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


    public class Wire
    {
        public static List<Wire> _registry = new List<Wire>();

        public int verticesAmount = 20;
        private int middlePointHeight = 8;
        public GameObject startObject;
        public GameObject endObject;
        public GameObject lineObject;
        public LineRenderer lineRenderer;
        public static Wire justCreated;

        public Wire(GameObject collideObject)
        {
            startObject = collideObject;
            CreateLineObject();
            justCreated = this;
        }

        public void SelectWire()
        {
            selectedWire = this;
            selectedWire.lineRenderer.material = highlightWireMaterial;
        }

        public static void UnselectWire()
        {
            if (selectedWire == null)
                return;
            selectedWire.lineRenderer.material = wireMaterial;
            selectedWire = null;
        }

        public void WireFollowMouse(Wire justCreated)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector3 targetPosition = MouseInteraction.GetNewPosition(cam, mousePosition, Vector2.zero, justCreated.lineRenderer.GetPosition(justCreated.verticesAmount-1));
            
            justCreated.lineRenderer.SetPosition(verticesAmount - 1, targetPosition);
            UpdateLinesOfWire();
        }

        private void CreateLineObject()
        {
            lineObject = new GameObject($"wire{_registry.Count + 1} [{startObject}]");
            lineRenderer = lineObject.AddComponent<LineRenderer>();
            lineRenderer.material = wireMaterial;
            lineRenderer.widthMultiplier = 0.1f;
            lineRenderer.positionCount = verticesAmount;

            //set all positions of line renderer
            Vector3[] positions = new Vector3[verticesAmount];
            for (int i = 0; i < verticesAmount; i++)
                positions[i] = startObject.transform.position;
            
            lineRenderer.SetPositions(positions);
            lineRenderer.numCapVertices = 4;

            UpdateLinesOfWire();
        }

        public void UpdateLinesOfWire()
        {
            Vector3 pos1 = lineRenderer.GetPosition(0);
            Vector3 pos2 = lineRenderer.GetPosition(verticesAmount-1);

            if (pos2 == pos1) return;

            Vector3 middle = (pos1 + pos2) / 2;
            middle.y = middlePointHeight;
            Vector3[] positions = CalculateVertices(pos1, middle, pos2, verticesAmount);
            positions[0] = pos1;
            positions[verticesAmount - 1] = pos2;
            lineRenderer.SetPositions(positions);
        }
    }

    static Vector3[] CalculateVertices(Vector3 from, Vector3 middle, Vector3 to, int vertices)
    {
        Vector3[] result = new Vector3[vertices];

        for (int i = 0; i < vertices; i++)
        {
            float t = i / (float)vertices;
            result[i] = CalculateQuadraticBezierPoint(t, from, middle, to);
        }
        return result;
    }

    static Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        // B(t) = (1-t)^2*p0 + 2*(1-t)*t*p1 + t^2*p2 , 0 < t < 1
        Vector3 result = ((1 - t)*(1 - t)) * p0 + 2 * (1 - t) * t * p1 + t*t * p2;
        return result;
    }

    /*
    public static bool IsPointWithinCollider(Collider collider, Vector3 point)
    {
        return collider.ClosestPoint(point) == point;
    }
    */

    public static Wire WireAlreadyExists(GameObject wireToObject)
    {
        foreach (Wire existingWire in Wire._registry)
        {
            if (wireToObject == existingWire.startObject
                || wireToObject == existingWire.endObject
                || wireToObject == existingWire.startObject
                || wireToObject == existingWire.endObject)
            {
                Debug.Log("WIRE ALREADY EXISTS!");
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

    private Material LoadMaterial(string name)
    {
        Material material = Resources.Load($"Materials/{name}", typeof(Material)) as Material;
        if (material == null)
            Debug.Log($"{name} not found!");
        return material;
    }

}
