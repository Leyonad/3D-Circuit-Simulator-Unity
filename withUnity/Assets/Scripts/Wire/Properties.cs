using System.Collections.Generic;
using UnityEngine;

public class Properties : MonoBehaviour
{
    //wire
    public double polarity; // + or - / 0 or 1

    //parentobject
    public double current; //in ampere

    //battery
    public double voltage;
    public double mAh;
    
    //wires attached to metal strip or item
    public List<Wire> attachedWires = new List<Wire>();

    //led
    public double voltageDrop; //in volts
    public double ampere; // 1 mA = 1/1000 ampere

    //resistor
    public double resistance; //in ohm
    public double tolerance; //percentage

    //optimization
    public Item item;
    public Wire wire;
    public Node node;
    public Battery battery;
}
