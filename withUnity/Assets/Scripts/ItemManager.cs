using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static bool itemSelected = false;
    public static float minItemY = 2f;
    public static float maxItemY = 6f;
    
    public static void Unselect()
    {
        LED.selectedLED = null;
        itemSelected = false;
    }
}
