using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

public class UIManager : MonoBehaviour
{
    public static Label propertiesTitle;

    public static TextField voltageField;
    public static TextField currentField;
    public static TextField resistanceField;
    public static TextField voltageDropField;

    //convert comma to dot when assigning values to ui elements
    private static NumberFormatInfo textFormat;

    public void Start()
    {
        //get the ui elements
        var root = GetComponent<UIDocument>().rootVisualElement;

        propertiesTitle = root.Q<Label>("propertiesTitle");
        voltageField = root.Q<TextField>("voltageField");
        currentField = root.Q<TextField>("currentField");
        resistanceField = root.Q<TextField>("resistanceField");
        voltageDropField = root.Q<TextField>("voltageDropField");

        textFormat = new() { NumberDecimalSeparator = "." };
    }

    public static void DisplayItemProperties(Item item)
    {
        //this method updates the values of the ui with the properties of an item
        SetLabelText(propertiesTitle, item.type);
        SetTextFieldValue(voltageField, item.itemObject.GetComponent<Properties>().voltage);
        SetTextFieldValue(currentField, item.itemObject.GetComponent<Properties>().current);
        SetTextFieldValue(resistanceField, item.itemObject.GetComponent<Properties>().resistance);
        SetTextFieldValue(voltageDropField, item.itemObject.GetComponent<Properties>().voltageDrop);
    }

    public static void DisplayMetalStripProperties(GameObject metalStripObj)
    {
        //this method updates the values of the ui with the properties of a metalstrip
        SetLabelText(propertiesTitle, "Metalstrip");
        SetTextFieldValue(voltageField, metalStripObj.GetComponent<Properties>().voltage);
        SetTextFieldValue(currentField, metalStripObj.GetComponent<Properties>().current);
        SetTextFieldValue(resistanceField, metalStripObj.GetComponent<Properties>().resistance);
        SetTextFieldValue(voltageDropField, metalStripObj.GetComponent<Properties>().voltageDrop);
    }

    public static void SetValuesToDefault()
    {
        //this method resets the values of the ui
        propertiesTitle.text = "";
        voltageField.value = "";
        currentField.value = "";
        resistanceField.value = "";
        voltageDropField.value = "";
    }

    public static void SetLabelText(Label label, string text)
    {
        //this method changes the text of a label
        label.text = text;
    }

    public static void SetTextFieldValue(TextField textField, double value)
    {
        //this method changes the value of a text field
        string unit = "";
        if (textField == voltageField || textField == voltageDropField) unit = "V";
        else if (textField == currentField) unit = "mA";
        else if (textField == resistanceField) unit = "Ω";

        textField.value = value.ToString(textFormat) + " " +unit;
    }

}
