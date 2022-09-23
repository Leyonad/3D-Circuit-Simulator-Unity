using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Wire
{
    public static List<Wire> _registry = new List<Wire>();
    public static int defaultVerticesAmount = 20;
    public int verticesAmount = defaultVerticesAmount;

    public readonly float wireMouseY = 1.1f;

    public float middlePointHeight = 4;
    public static float minMiddlePointHeight = 2f;
    public static float maxMiddlePointHeight = 12f;

    public GameObject startObject;
    public GameObject endObject;
    public GameObject lineObject;
    public LineRenderer lineRenderer;
    public static Wire justCreated;

    MeshCollider meshCollider;
    Mesh mesh;

    //for updateElectricityParameters() to check if wire has been visited
    public bool updated = false;

    public bool flat = false;
    public static float flatHeight = 1.25f;

    public Wire(GameObject obj1, GameObject obj2=null, float _middlePointHeight=0f)
    {
        startObject = obj1;
        endObject = obj2;
        if (_middlePointHeight > 0)
            middlePointHeight = _middlePointHeight;
        if (obj2 == null)
            justCreated = this;
        CreateLineObject();
        UpdateLinesOfWire();
        meshCollider = lineObject.AddComponent<MeshCollider>();
        mesh = new Mesh();
    }

    public void WireFollowMouse(Wire wire)
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 targetPosition = MouseInteraction.GetNewPosition(mousePosition, Vector2.zero, wire.lineRenderer.GetPosition(justCreated.verticesAmount - 1));

        wire.lineRenderer.SetPosition(verticesAmount - 1, targetPosition);
        UpdateLinesOfWire();
    }

    private void CreateLineObject()
    {
        lineObject = new GameObject($"wire{_registry.Count + 1} [{startObject}]")
        {
            tag = "Wire"
        };

        lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.material = ResourcesManager.wireMaterial;
        lineRenderer.widthMultiplier = 0.1f;
        lineRenderer.positionCount = verticesAmount;

        //set all positions of line renderer
        Vector3[] positions = new Vector3[verticesAmount];
        for (int i = 0; i < verticesAmount; i++)
            positions[i] = new Vector3(startObject.transform.position.x, wireMouseY, startObject.transform.position.z);

        lineRenderer.SetPositions(positions);
        lineRenderer.numCapVertices = 4;
    }

    public float HasCurrent()
    {
        if (this.startObject.transform.parent.GetComponent<Properties>().current != 0f)
            return this.startObject.transform.parent.GetComponent<Properties>().current;
        if (this.endObject.transform.parent.GetComponent<Properties>().current != 0f)
            return this.endObject.transform.parent.GetComponent<Properties>().current;

        return 0f;
    }

    public void SetCurrent(GameObject obj, float current)
    {
        obj.transform.parent.GetComponent<Properties>().current = current;
    }

    public void UpdateMeshOfWire()
    {
        lineRenderer.BakeMesh(mesh, GameManager.cam);
        meshCollider.sharedMesh = mesh;
    }

    public static void UpdateAllMeshes()
    {
        foreach (Wire wire in Wire._registry)
        {
            wire.UpdateMeshOfWire();
        }
    }

    public void MakeWireFlat()
    {
        Vector3 vec3FlatHeight = Vector3.up * flatHeight;
        Vector3[] temp = {
                        lineRenderer.GetPosition(0),
                        new Vector3(lineRenderer.GetPosition(0).x, vec3FlatHeight.y, lineRenderer.GetPosition(0).z),
                        new Vector3(lineRenderer.GetPosition(verticesAmount-1).x, vec3FlatHeight.y, lineRenderer.GetPosition(verticesAmount-1).z),
                        lineRenderer.GetPosition(verticesAmount - 1)
                    };
        verticesAmount = 4;
        lineRenderer.positionCount = 4;
        lineRenderer.SetPositions(temp);
        UpdateMeshOfWire();
    }

    public void MakeWireCurve()
    {
        Vector3 startPos = lineRenderer.GetPosition(0);
        Vector3 endPos = lineRenderer.GetPosition(verticesAmount - 1);

        verticesAmount = defaultVerticesAmount;
        lineRenderer.positionCount = 0;
        lineRenderer.positionCount = defaultVerticesAmount;
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(verticesAmount-1, endPos);
        UpdateLinesOfWire();
        UpdateMeshOfWire();
    }

    public void UpdateLinesOfWire()
    {
        if (flat)
        {
            Vector3 vec3FlatHeight = Vector3.up * flatHeight;
            lineRenderer.SetPosition(1, new Vector3(lineRenderer.GetPosition(0).x, vec3FlatHeight.y, lineRenderer.GetPosition(0).z));
            lineRenderer.SetPosition(verticesAmount-2, new Vector3(lineRenderer.GetPosition(verticesAmount-1).x, vec3FlatHeight.y, lineRenderer.GetPosition(verticesAmount-1).z));
            UpdateMeshOfWire();
            return;
        }

        Vector3 pos1 = lineRenderer.GetPosition(0);
        Vector3 pos2 = lineRenderer.GetPosition(verticesAmount - 1);

        if (pos2 == pos1) return; 
        Vector3 middle = (pos1 + pos2) / 2;
        middle.y = middlePointHeight;
        Vector3[] positions = CalculateVertices(pos1, middle, pos2, verticesAmount);
        positions[0] = pos1;
        positions[verticesAmount - 1] = pos2;
        lineRenderer.SetPositions(positions);
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
        Vector3 result = ((1 - t) * (1 - t)) * p0 + 2 * (1 - t) * t * p1 + t * t * p2;
        return result;
    }

    public static void UpdateWiresPosition(GameObject obj)
    {
        //this method updates the wire positions when a component is being moved
        foreach (Wire wire in Wire._registry)
        {
            //if wire is a part of an item, skip, since the wire is updated in a seperate function
            if (wire.lineObject.transform.parent != null && wire.lineObject.transform.parent.CompareTag("Item")) 
                continue;
            else if (wire.startObject.transform.IsChildOf(obj.transform) || wire.endObject.transform.IsChildOf(obj.transform))
            {
                wire.lineRenderer.SetPosition(0, wire.startObject.transform.position);
                wire.lineRenderer.SetPosition(wire.verticesAmount - 1, wire.endObject.transform.position);
                wire.UpdateLinesOfWire();
            }
        }
    }

    public void FlipStartEndOfWire()
    {
        Vector3 temp = lineRenderer.GetPosition(0);
        lineRenderer.SetPosition(0, lineRenderer.GetPosition(verticesAmount - 1));
        lineRenderer.SetPosition(verticesAmount - 1, temp);
    }


    public void AttachToParent()
    {
        startObject.transform.parent.gameObject.GetComponent<Properties>().attachedWires.Add(this);
        endObject.transform.parent.gameObject.GetComponent<Properties>().attachedWires.Add(this);
    }


    public void FinishWireCreation()
    {
        AttachToParent();
        UpdateLinesOfWire();
        UpdateMeshOfWire();
        _registry.Add(this);
    }
}
