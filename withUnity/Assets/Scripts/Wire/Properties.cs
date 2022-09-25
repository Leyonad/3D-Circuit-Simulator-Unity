using System.Collections.Generic;
using UnityEngine;

public class Properties : MonoBehaviour
{
    //wire
    public float current;

    //battery
    public float voltage;
    public float mAh;
    
    //wires attached to metal strip or item
    public List<Wire> attachedWires = new List<Wire>();

    //led
    public float voltageDrop; //in volts
    public float ampere; // 1 mA = 1/1000 ampere

    //resistor
    public float resistance; //in ohm
    public float tolerance; //percentage

    //optimization
    public Item item;
    public Wire wire;
}
