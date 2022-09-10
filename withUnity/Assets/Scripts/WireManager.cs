using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class WireManager : MonoBehaviour
{
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
                //Check if wire doesnt already exist
                bool wireAlreadyExists = false;
                foreach (Wire wire in Wire._registry)
                {
                    if (Wire.justCreated.verticesOfWire.FirstOrDefault() == wire.verticesOfWire.FirstOrDefault() 
                      && Wire.justCreated.verticesOfWire.LastOrDefault() == wire.verticesOfWire.LastOrDefault())
                    {
                        wireAlreadyExists = true;
                        break;
                    }
                        
                }
                if (!wireAlreadyExists) {
                    Debug.Log("New Wire created");
                    Wire._registry.Add(Wire.justCreated);
                }
                else {
                    Destroy(Wire.justCreated.lineObject);
                }

                Wire.justCreated = null;
            }
        }
    }

    public class Wire
    {
        public static List<Wire> _registry = new List<Wire>();
        public List<Vector3> verticesOfWire = new List<Vector3>();
        public GameObject lineObject;
        LineRenderer lineRenderer;
        public static Wire justCreated;

        public Wire(GameObject startObject)
        {   
            //Later do this in a loop
            verticesOfWire.Add(startObject.transform.position);
            verticesOfWire.Add(startObject.transform.position);

            createLineObject();
            justCreated = this;
        }

        public void wireFollowMouse(Wire justCreated)
        {
            Vector3 mousePositionWorld = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            justCreated.lineRenderer.SetPosition(verticesOfWire.Count-1, mousePositionWorld);

            //update the other vertices of the line based on the start and end position
            //with kinetic equation (Sebastian Lague Video)

        }

        private void createLineObject()
        {
            lineObject = new GameObject($"wire{_registry.Count + 1}");
            lineRenderer = lineObject.AddComponent<LineRenderer>();
            lineRenderer.material = wireMaterial;
            lineRenderer.widthMultiplier = 0.1f;
            lineRenderer.numCapVertices = 4;
            
            int i = 0;
            foreach (Vector3 pos in verticesOfWire)
            {
                lineRenderer.SetPosition(i, pos);
                i++;
            }
        }
    }


}
