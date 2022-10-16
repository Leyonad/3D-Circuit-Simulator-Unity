using UnityEngine;

public class Functions
{
    public static float GetYRotationBetween2Points(Vector3 p1, Vector3 p2)
    {
        float xDiff = p2.x - p1.x;
        float yDiff = p2.z - p1.z;
        return Mathf.Atan2(yDiff, xDiff) * 180.0f / Mathf.PI;
    }

    public static float MapToRange(float x, float rangeMin, float rangeMax, float outputMinRange, float outputMaxRange)
    {
        return (x - rangeMin) * (outputMaxRange - outputMinRange) / (rangeMax - rangeMin) + outputMinRange;
    }

    private const byte k_MaxByteForOverexposedColor = 191; //internal Unity const 

    public static Color ChangeHDRColorIntensity(Color currentColor, float newIntensity)
    {
        // Get color intensity
        float maxColorComponent = currentColor.maxColorComponent;
        float scaleFactorToGetIntensity = k_MaxByteForOverexposedColor / maxColorComponent;
        float currentIntensity = Mathf.Log(255f / scaleFactorToGetIntensity) / Mathf.Log(2f);

        // Get original color with ZERO intensity
        float currentScaleFactor = Mathf.Pow(2, currentIntensity);
        Color originalColorWithoutIntensity = currentColor / currentScaleFactor;

        // Add color intensity
        float modifiedIntensity = newIntensity;

        // Set color intensity
        float newScaleFactor = Mathf.Pow(2, modifiedIntensity);
        Color colorToRetun = originalColorWithoutIntensity * newScaleFactor;

        // Return color
        return colorToRetun;
    }

    public static Vector3 KeepObjectInBounds(GameObject obj, float xmin, float xmax, float zmin, float zmax)
    {
        if (obj.transform.position.x > xmax)
            obj.transform.position = new Vector3(xmax, obj.transform.position.y, obj.transform.position.z);
        else if (obj.transform.position.x < xmin)
            obj.transform.position = new Vector3(xmin, obj.transform.position.y, obj.transform.position.z);
        if (obj.transform.position.z > zmax)
            obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y, zmax);
        else if (obj.transform.position.z < zmin)
            obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y, zmin);
        
        return obj.transform.position;
    }
}
