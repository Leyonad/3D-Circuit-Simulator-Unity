﻿using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    //notification if an error occurs (singular matrix)
    public static VisualElement warningNotification;
    public static Label warningNotificationText;

    //properties segment
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

        warningNotification = root.Q<VisualElement>("WarningNotification");
        warningNotificationText = root.Q<Label>("NotificationText");

        textFormat = new() { NumberDecimalSeparator = "." };
    }

    public static void DisplayItemProperties(Item item)
    {
        //this method updates the values of the ui with the properties of an item
        SetLabelText(propertiesTitle, item.type);
        SetTextFieldValue(currentField, item.itemObject.GetComponent<Properties>().current);
        SetTextFieldValue(resistanceField, item.itemObject.GetComponent<Properties>().resistance * 1000);
        SetTextFieldValue(voltageDropField, item.itemObject.GetComponent<Properties>().voltageDrop);
        SetValueToDefault(voltageField);

        if (item.type == "LED")
            SetValueToDefault(resistanceField);
        else if (item.type == "Resistor")
            SetValueToDefault(voltageDropField);

    }

    public static void DisplayWireProperties(Wire wire)
    {
        //this method updates the values of the ui with the properties of a wire
        SetLabelText(propertiesTitle, "Wire");
        SetTextFieldValue(currentField, wire.lineObject.GetComponent<Properties>().current);

        SetValueToDefault(voltageField);
        SetValueToDefault(resistanceField);
        SetValueToDefault(voltageDropField);
    }

    public static void DisplayMetalStripProperties(GameObject metalStripObj)
    {
        //this method updates the values of the ui with the properties of a metalstrip
        SetLabelText(propertiesTitle, "Metalstrip");
        SetTextFieldValue(voltageField, metalStripObj.GetComponent<Properties>().voltage);
        SetTextFieldValue(currentField, metalStripObj.GetComponent<Properties>().current);

        SetValueToDefault(resistanceField);
        SetValueToDefault(voltageDropField);
    }

    public static void SetValueToDefault(TextField textField)
    {
        //this method resets one value of the ui
        textField.value = "";
    }

    public static void SetAllValuesToDefault()
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

    public static void SendWarningNotification(string message)
    {
        warningNotificationText.text = message;
        warningNotification.style.display = DisplayStyle.Flex;
    }

    public static void StopWarningNotification()
    {
        warningNotification.style.display = DisplayStyle.None;
    }

}
