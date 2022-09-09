using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LineDrawer : MonoBehaviour
{
    private LineRenderer lineRend;
    [SerializeField] private Camera cam;

    void Start()
    {
        lineRend = GetComponent<LineRenderer>();
    }

    void Update()
    {
        //if(Mouse.current.leftButton.wasPressedThisFrame)
        Vector3 mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        lineRend.SetPosition(0, new Vector3(1.5f, 0f, 0.449f));
        lineRend.SetPosition(1, mousePos);
    }
}
