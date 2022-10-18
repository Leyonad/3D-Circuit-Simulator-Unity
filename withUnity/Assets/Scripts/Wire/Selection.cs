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
        wire = _wire;
        item = _item;

        if (_wire != null)
        {
            SaveCurrentWireMaterials();
            SetNewWireMaterial();
            currentlySelectedWires.Add(this);
            oneWireIsSelected = true;
        }
        else if (_item != null)
        {
            SaveCurrentItemMaterial();
            SetNewItemMaterial();
            currentlySelectedItems.Add(this);
            oneItemIsSelected = true;

            //set values of ui properties
            UIManager.SetValue(UIManager.voltageField, _item.itemObject.GetComponent<Properties>().voltage);
            UIManager.SetValue(UIManager.currentField, _item.itemObject.GetComponent<Properties>().current);
            UIManager.SetValue(UIManager.resistanceField, _item.itemObject.GetComponent<Properties>().resistance);
            UIManager.SetValue(UIManager.voltageDropField, _item.itemObject.GetComponent<Properties>().voltageDrop);
        }


    }

    public static void UnselectSelection()
    {
        if (oneWireIsSelected) UnselectAllWires();
        if (oneItemIsSelected) UnselectAllItems();

        UIManager.SetValuesToDefault();
    }

    public static void DeleteSelection()
    {
        if (oneWireIsSelected) DeleteSelectedWires();
        if (oneItemIsSelected) DeleteSelectedItems();
    }

    public static void DeleteSelectedWires()
    {
        foreach (Selection current in currentlySelectedWires)
        {
            ResetMetalsMaterial(current);
            current.wire.startObject.transform.parent.GetComponent<Properties>().attachedWires.Remove(current.wire);
            current.wire.endObject.transform.parent.GetComponent<Properties>().attachedWires.Remove(current.wire);
            WireManager.connectedWires.Remove(current.wire);
            Wire._registry.Remove(current.wire);
            Object.Destroy(current.wire.lineObject);
        }
        currentlySelectedWires.Clear();
        oneWireIsSelected = false;
    }

    public static void DeleteSelectedItems()
    {
        foreach (Selection current in currentlySelectedItems)
        {
            Item._registry.Remove(current.item);
            Object.Destroy(current.item.itemObject);
        }
        currentlySelectedItems.Clear();
        oneItemIsSelected = false;
    }

    private static void UnselectAllWires()
    {
        foreach (Selection current in currentlySelectedWires)
        {
            ResetWireMaterial(current);
            ResetMetalsMaterial(current);
        }
        currentlySelectedWires.Clear();
        oneWireIsSelected = false;
    }

    private static void UnselectAllItems()
    {
        foreach (Selection current in currentlySelectedItems)
        {
            ResetItemMaterial(current);
        }
        currentlySelectedItems.Clear();
        oneItemIsSelected = false;
    }

    private static void ResetItemMaterial(Selection current)
    {
        current.item.itemObject.GetComponent<MeshRenderer>().material = current.previousMaterial;
    }

    private static void ResetWireMaterial(Selection current)
    {
        current.wire.lineRenderer.material = current.previousMaterial;
    }

    private static void ResetMetalsMaterial(Selection current)
    {
        current.wire.startObject.GetComponent<MeshRenderer>().material = current.previousStartMetalMaterial;
        current.wire.endObject.GetComponent<MeshRenderer>().material = current.previousEndMetalMaterial;
    }

    private void SetNewItemMaterial()
    {
        item.itemObject.GetComponent<MeshRenderer>().material = ResourcesManager.highlightItemMaterial;
    }

    private void SaveCurrentItemMaterial()
    {
        previousMaterial = item.itemObject.GetComponent<MeshRenderer>().material;
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
}
