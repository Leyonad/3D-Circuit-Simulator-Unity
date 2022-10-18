using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static TextField voltageField;
    public static TextField currentField;
    public static TextField resistanceField;
    public static TextField voltageDropField;

    private static string defaultValue = "...";

    //convert comma to dot when assigning values to ui elements
    private static NumberFormatInfo textFormat;

    public void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        voltageField = root.Q<TextField>("voltageField");
        currentField = root.Q<TextField>("currentField");
        resistanceField = root.Q<TextField>("resistanceField");
        voltageDropField = root.Q<TextField>("voltageDropField");

        textFormat = new() { NumberDecimalSeparator = "." };
    }

    public static void SetValuesToDefault()
    {
        voltageField.value = defaultValue + " V";
        currentField.value = defaultValue + " mA";
        resistanceField.value = defaultValue + " Ω";
        voltageDropField.value = defaultValue + " V";
    }

    public static void SetValue(TextField textField, double value)
    {
        string unit = "";
        if (textField == voltageField || textField == voltageDropField) unit = "V";
        else if (textField == currentField) unit = "mA";
        else if (textField == resistanceField) unit = "Ω";

        textField.value = value.ToString(textFormat) + " " +unit;
    }

}
