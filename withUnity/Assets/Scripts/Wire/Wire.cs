using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    MeshCollider meshCollider;
    Mesh mesh;

    //for updateElectricityParameters() to check if wire has been visited
    public bool updated = false;

    public Wire(GameObject collideObject)
    {
        startObject = collideObject;
        CreateLineObject();
        justCreated = this;
    }

    public void WireFollowMouse(Wire justCreated)
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 targetPosition = MouseInteraction.GetNewPosition(WireManager.cam, mousePosition, Vector2.zero, justCreated.lineRenderer.GetPosition(justCreated.verticesAmount - 1));

        justCreated.lineRenderer.SetPosition(verticesAmount - 1, targetPosition);
        UpdateLinesOfWire();
    }

    private void CreateLineObject()
    {
        lineObject = new GameObject($"wire{_registry.Count + 1} [{startObject}]")
        {
            tag = "Wire"
        };

        //NO BOX COLLIDER ??
        lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.material = ResourcesManager.wireMaterial;
        lineRenderer.widthMultiplier = 0.1f;
        lineRenderer.positionCount = verticesAmount;

        //set all positions of line renderer
        Vector3[] positions = new Vector3[verticesAmount];
        for (int i = 0; i < verticesAmount; i++)
            positions[i] = startObject.transform.position;

        lineRenderer.SetPositions(positions);
        lineRenderer.numCapVertices = 4;

        UpdateLinesOfWire();

        meshCollider = lineObject.AddComponent<MeshCollider>();
        mesh = new Mesh();
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
        lineRenderer.BakeMesh(mesh, WireManager.cam);
        meshCollider.sharedMesh = mesh;
    }

    public static void UpdateAllMeshes()
    {
        foreach (Wire wire in Wire._registry)
        {
            wire.UpdateMeshOfWire();
        }
    }

    public void UpdateLinesOfWire()
    {
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
}
