using System.Collections.Generic;
using UnityEngine;

public class Selection
{
    public static bool multiSelectionEnabled = false;
    public Material previousMaterial;

    //Wire selection
    public Wire wire;
    public Material previousStartMetalMaterial;
    public Material previousEndMetalMaterial;
    public static bool oneWireIsSelected = false;
    public static List<Selection> currentlySelectedWires = new List<Selection>();

    //Item selection
    public Item item;
    public static bool oneItemIsSelected = false;
    public static List<Selection> currentlySelectedItems = new List<Selection>();

    public Selection(Wire _wire=null, Item _item=null)
    {
        if (_wire != null)
        {
            wire = _wire;
            SaveCurrentWireMaterials();
            SetNewWireMaterial();
            currentlySelectedWires.Add(this);
            oneWireIsSelected = true;
        }
        else if (_item != null)
        {
            item = _item;
            SaveCurrentItemMaterial();
            SetNewItemMaterial();
            currentlySelectedItems.Add(this);
            oneItemIsSelected = true;
        }

    }

    private void SetNewItemMaterial()
    {
        item.itemObject.GetComponent<MeshRenderer>().material = ResourcesManager.highlightItemMaterial;
    }

    private void SaveCurrentItemMaterial()
    {
        previousMaterial = item.itemMaterial;
    }

    private void SetNewWireMaterial()
    {
        wire.lineRenderer.material = ResourcesManager.highlightWireMaterial;
        wire.startObject.GetComponent<MeshRenderer>().material = ResourcesManager.highlightWireMaterial;
        wire.endObject.GetComponent<MeshRenderer>().material = ResourcesManager.highlightWireMaterial;
    }

    private void SaveCurrentWireMaterials()
    {
        previousMaterial = wire.lineRenderer.material;
        previousStartMetalMaterial = wire.startObject.GetComponent<MeshRenderer>().material;
        previousEndMetalMaterial = wire.endObject.GetComponent<MeshRenderer>().material;
    }

    public static void UnselectSelection()
    {
        if (oneWireIsSelected) UnselectAllWires();
        if (oneItemIsSelected) UnselectAllItems();

    }
    private static void UnselectAllWires()
    {
        foreach (Selection current in currentlySelectedWires)
        {
            current.wire.lineRenderer.material = current.previousMaterial;
            current.wire.startObject.GetComponent<MeshRenderer>().material = current.previousStartMetalMaterial;
            current.wire.endObject.GetComponent<MeshRenderer>().material = current.previousEndMetalMaterial;
        }
        currentlySelectedWires.Clear();
        oneWireIsSelected = false;
    }

    private static void UnselectAllItems()
    {
        foreach (Selection current in currentlySelectedItems)
        {
            current.item.itemObject.GetComponent<MeshRenderer>().material = current.previousMaterial;
        }
        currentlySelectedItems.Clear();
        oneItemIsSelected = false;
    }
}
