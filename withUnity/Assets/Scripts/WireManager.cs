using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireManager : MonoBehaviour
{
    public static Material wireMaterial;
    public int numCapVertices = 4;

    private void Start()
    {
        wireMaterial = Resources.Load("Materials/Wire_Material", typeof(Material)) as Material;
        Debug.Log(wireMaterial);
    }

    public class Wire
    {
        public static List<Wire> _registry = new List<Wire>();
        public Vector3 pos1;
        public Vector3 pos2;
        GameObject newWire;

        public Wire(GameObject startObject)
        {
            pos1 = startObject.transform.position;
            pos2 = new Vector3(5f, 5f, 5f);

            newWire = new GameObject($"wire{_registry.Count+1}");
            createLineRenderer();

            _registry.Add(this);
            Debug.Log("New Wire(): pos1 = " + pos1 + " pos2 = " + pos2);
        }

        private void createLineRenderer()
        {
            LineRenderer lineRenderer = newWire.AddComponent<LineRenderer>();
            lineRenderer.material = wireMaterial;
            lineRenderer.widthMultiplier = 0.1f;
            lineRenderer.numCapVertices = 4;
            lineRenderer.SetPosition(0, pos1);
            lineRenderer.SetPosition(1, pos2);
        }
    }


}
