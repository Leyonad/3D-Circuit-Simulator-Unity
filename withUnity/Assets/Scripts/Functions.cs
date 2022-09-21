using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Functions
{
    public static float GetYRotationBetween2Points(Vector3 p1, Vector3 p2)
    {
        float xDiff = p2.x - p1.x;
        float yDiff = p2.z - p1.z;
        return Mathf.Atan2(yDiff, xDiff) * 180.0f / Mathf.PI;
    }
}
