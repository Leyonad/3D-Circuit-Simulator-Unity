using System.Collections.Generic;
using UnityEngine;

public class Properties : MonoBehaviour
{
    //ground
    public bool ground = false;

    //wire
    public double polarity; // + or - / 0 or 1

    //parentobject
    public double current; //in milliampere

    //battery
    public double voltage;
    public double mAh;
    
    //wires attached to metal strip or item
    public List<Wire> attachedWires = new List<Wire>();

    //led
    public double voltageDrop; //in volts
    public double requiredCurrent; // in milliampere
    public double minCurrent; // in milliampere
    public double maxCurrent; // in milliampere

    //resistor
    public double resistance; //in ohm
    public double tolerance; //percentage

    //optimization
    public Item item;
    public Wire wire;
    public Node node;
    public Battery battery;
}
