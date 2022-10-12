using UnityEngine;
using UnityEngine.EventSystems;

public class MouseHovering : MonoBehaviour, 
            IPointerEnterHandler, IPointerExitHandler
{
    Color objectsColor;
    public Color hoveringColor = Color.red;
    new Renderer renderer;

    private void Start()
    {
        objectsColor = GetComponent<Renderer>().material.color;
        renderer = GetComponent<Renderer>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        renderer.material.color = hoveringColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        renderer.material.color = objectsColor;
    }

}
