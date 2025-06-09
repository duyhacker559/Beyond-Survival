using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SimpleDrag : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool isDragging = false;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    void Update()
    {
        if (isDragging)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform.parent as RectTransform,
                Input.mousePosition,
                null,
                out pos
            );
            rectTransform.anchoredPosition = pos;
        }
    }
}

public static class DragPayload
{
    public static GameObject CurrentDraggedIcon;
    public static ItemInstance ItemData;
    public static Sprite icon;

    public static string dragType = "";
    public static GameObject dragger;
}
