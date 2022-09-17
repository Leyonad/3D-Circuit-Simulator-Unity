using System.Collections.Generic;
using UnityEngine;

public class Properties : MonoBehaviour
{
    public float voltage;
    public float current;
    public List<Wire> attachedWires = new List<Wire>();
}
