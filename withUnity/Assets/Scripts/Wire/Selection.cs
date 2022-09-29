using System.Collections.Generic;
using UnityEngine;

public class Selection
{
    public static List<Selection> currentlySelectedWires = new List<Selection>();
    public static bool oneWireIsSelected = false;

    public Wire wire;
    public Material previousWireMaterial;
    public Material previousStartMetalMaterial;
    public Material previousEndMetalMaterial;

    public Selection(Wire _wire)
    {
        wire = _wire;
        SaveCurrentWireMaterials();
        SetNewWireMaterials();

        currentlySelectedWires.Add(this);
        oneWireIsSelected = true;
    }

    private void SetNewWireMaterials()
    {
        wire.lineRenderer.material = ResourcesManager.highlightWireMaterial;
        wire.startObject.GetComponent<MeshRenderer>().material = ResourcesManager.highlightWireMaterial;
        wire.endObject.GetComponent<MeshRenderer>().material = ResourcesManager.highlightWireMaterial;
    }

    private void SaveCurrentWireMaterials()
    {
        previousWireMaterial = wire.lineRenderer.material;
        previousStartMetalMaterial = wire.startObject.GetComponent<MeshRenderer>().material;
        previousEndMetalMaterial = wire.endObject.GetComponent<MeshRenderer>().material;
    }

    public static void UnselectSelection()
    {
        foreach (Selection current in currentlySelectedWires)
        {
            current.wire.lineRenderer.material = current.previousWireMaterial;
            current.wire.startObject.GetComponent<MeshRenderer>().material = current.previousStartMetalMaterial;
            current.wire.endObject.GetComponent<MeshRenderer>().material = current.previousEndMetalMaterial;
        }
        currentlySelectedWires.Clear();
        oneWireIsSelected = false;
    }
}
