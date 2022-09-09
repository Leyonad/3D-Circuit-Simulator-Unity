using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire{

    List<Wire> _registry = new List<Wire>();
    GameObject startObject;
    GameObject endObject;

    public Wire(GameObject startObject)
    {
        this.startObject = startObject;
        //GameObject.CreatePrimitive()
        //Debug.Log("new wire object created with startobject = " + startObject);
    }

}
