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

    private void Start()
    {
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
            Wire.justCreated.wireFollowMouse(Wire.justCreated);

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
                    //add last point to the vertices of the new wire
                    Wire.justCreated.verticesOfWire[Wire.justCreated.verticesAmount - 1] =
                        hit.collider.gameObject.transform.position;

                    //Check if the wire doesnt already exist
                    if (!WireAlreadyExists(Wire.justCreated))
                    {
                        Debug.Log("New Wire created " + Wire.justCreated.verticesOfWire.First() +
                                    " to " + Wire.justCreated.verticesOfWire.Last());
                        Wire.justCreated.updateLinesOfWire();
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

        public List<Vector3> verticesOfWire = new List<Vector3>();
        public int verticesAmount = 2;
        public string wireTag;
        public GameObject lineObject;
        private LineRenderer lineRenderer;
        public static Wire justCreated;

        public Wire(GameObject startObject, string tag)
        {
            //Later do this in a loop
            //start and end positions must be the same in the beginning
            //end position changes later to mouse position
            verticesOfWire.Add(startObject.transform.position);
            verticesOfWire.Add(startObject.transform.position);
            if (!WireAlreadyExists(this))
            {
                wireTag = tag;
                createLineObject();
                justCreated = this;
            }
        }

        public void wireFollowMouse(Wire justCreated)
        {
            Vector3 mousePositionWorld = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            justCreated.lineRenderer.SetPosition(verticesAmount - 1, mousePositionWorld);
        }

        private void createLineObject()
        {
            lineObject = new GameObject($"wire{_registry.Count + 1} [{wireTag}]");
            lineObject.tag = wireTag;
            lineRenderer = lineObject.AddComponent<LineRenderer>();
            lineRenderer.material = wireMaterial;
            lineRenderer.widthMultiplier = 0.1f;
            lineRenderer.numCapVertices = 4;

            updateLinesOfWire();
        }

        public void updateLinesOfWire()
        {
            int i = 0;
            foreach (Vector3 pos in verticesOfWire)
            {
                lineRenderer.SetPosition(i, pos);
                i++;
            }
        }
    }

    public static bool WireAlreadyExists(Wire wire)
    {
        foreach (Wire existingWire in Wire._registry)
        {
            if (wire.verticesOfWire.First() == existingWire.verticesOfWire.First()
                || wire.verticesOfWire.First() == existingWire.verticesOfWire.Last()
                || wire.verticesOfWire.Last() == existingWire.verticesOfWire.First()
                || wire.verticesOfWire.Last() == existingWire.verticesOfWire.Last())
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
