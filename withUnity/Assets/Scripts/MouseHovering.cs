using UnityEngine;
using UnityEngine.EventSystems;

public class MouseHovering : MonoBehaviour, 
            IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    Color objectsColor;
    public Color hoveringColor = Color.red;
    new Renderer renderer;
    static bool click = false;

    private void Start()
    {
        objectsColor = GetComponent<Renderer>().material.color;
        renderer = GetComponent<Renderer>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!click) renderer.material.color = hoveringColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!click) renderer.material.color = objectsColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        click = true;
        //renderer.material.color = objectsColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        click = false;
        //renderer.material.color = hoveringColor;
    }

}
