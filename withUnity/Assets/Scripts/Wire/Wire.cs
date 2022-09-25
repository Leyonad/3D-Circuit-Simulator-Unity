using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Wire
{
    public static List<Wire> _registry = new List<Wire>();
    public static int defaultVerticesAmount = 20;
    public int verticesAmount = defaultVerticesAmount;

    public readonly float wireMouseY = 1.1f;

    public float middlePointHeight = 4f;
    public static float minMiddlePointHeight = 2f;
    public static float maxMiddlePointHeight = 12f;

    public GameObject startObject;
    public GameObject endObject;
    public GameObject lineObject;
    public LineRenderer lineRenderer;
    public static Wire justCreated;
    public Material wireColor;
    public float wireThickness;

    public Item parentItem = null;

    MeshCollider meshCollider;
    Mesh mesh;

    //for updateElectricityParameters() to check if wire has been visited
    public bool updated = false;

    public bool flat = false;
    public static float flatHeight = 1.25f;

    public Wire(GameObject obj1, GameObject obj2=null, float _middlePointHeight=0f, Item _item=null)
    {
        parentItem = _item;
        startObject = obj1;
        endObject = obj2;

        if (_middlePointHeight > 0f)
            middlePointHeight = _middlePointHeight;
        if (obj2 == null)
            justCreated = this;

        if (_item != null)
        {
            wireThickness = _item.wireThickness;
            wireColor = _item.wireColor;
        }
        else
        {
            wireThickness = 0.08f;
            wireColor = ResourcesManager.wireMaterial;
        }

        CreateLineObject();
        UpdatePointsOfWire();
        meshCollider = lineObject.AddComponent<MeshCollider>();
        mesh = new Mesh();
    }

    public void WireFollowMouse(Wire wire)
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 targetPosition = MouseInteraction.GetNewPosition(mousePosition, Vector2.zero, wire.lineRenderer.GetPosition(justCreated.verticesAmount - 1));

        wire.lineRenderer.SetPosition(verticesAmount - 1, targetPosition);
        UpdatePointsOfWire();
    }

    private void CreateLineObject()
    {
        lineObject = new GameObject($"wire{_registry.Count + 1} [{startObject}]")
        {
            tag = "Wire"
        };

        lineObject.AddComponent<Properties>();
        lineObject.GetComponent<Properties>().wire = this;
        
        lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.material = wireColor;
        lineRenderer.widthMultiplier = wireThickness;
        lineRenderer.positionCount = verticesAmount;

        //set all positions of line renderer
        Vector3[] positions = new Vector3[verticesAmount];
        Vector3 initPosition;

        //get initial position for all points
        if (parentItem != null) initPosition = parentItem.startObject.transform.position;
        else initPosition = startObject.transform.position;

        //set position of all points to initial position
        for (int i = 0; i < verticesAmount; i++)
            positions[i] = initPosition;

        lineRenderer.SetPositions(positions);
        lineRenderer.numCapVertices = 4;
    }

    public static void UpdateWirePositionComponent(GameObject obj)
    {
        //this method updates the wire positions when a component is being moved
        foreach (Wire wire in Wire._registry)
        {
            if (wire.startObject.transform.IsChildOf(obj.transform) || wire.endObject.transform.IsChildOf(obj.transform))
                wire.UpdateWirePosition();
        }
    }

    public void UpdateWirePosition()
    {
        this.lineRenderer.SetPosition(0, this.startObject.transform.position);
        this.lineRenderer.SetPosition(this.verticesAmount - 1, this.endObject.transform.position);
        this.UpdatePointsOfWire();
    }

    public void UpdatePointsOfWire()
    {
        if (flat)
        {
            Vector3 vec3FlatHeight = Vector3.up * flatHeight;
            lineRenderer.SetPosition(1, new Vector3(lineRenderer.GetPosition(0).x, vec3FlatHeight.y, lineRenderer.GetPosition(0).z));
            lineRenderer.SetPosition(verticesAmount - 2, new Vector3(lineRenderer.GetPosition(verticesAmount - 1).x, vec3FlatHeight.y, lineRenderer.GetPosition(verticesAmount - 1).z));
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
        lineRenderer.SetPosition(verticesAmount - 1, endPos);
        UpdatePointsOfWire();
        UpdateMeshOfWire();
    }

    public void AttachToParent()
    {
        startObject.transform.parent.gameObject.GetComponent<Properties>().attachedWires.Add(this);
        endObject.transform.parent.gameObject.GetComponent<Properties>().attachedWires.Add(this);
    }

    public void FinishWireCreation()
    {
        AttachToParent();
        UpdateWirePosition();
        UpdateMeshOfWire();
        _registry.Add(this);
    }

    public void FlipStartEndOfWire()
    {
        Vector3 temp = lineRenderer.GetPosition(0);
        lineRenderer.SetPosition(0, lineRenderer.GetPosition(verticesAmount - 1));
        lineRenderer.SetPosition(verticesAmount - 1, temp);
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
}
