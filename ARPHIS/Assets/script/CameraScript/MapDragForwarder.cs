using UnityEngine;
using UnityEngine.EventSystems;

public class MapDragForwarder : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public MapCameraPan mapCameraPan; // Drag MapCameraHolder here
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Forward to MapCameraPan if needed
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        // Forward the drag event to MapCameraPan
        if (mapCameraPan != null)
            mapCameraPan.OnDrag(eventData);
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        // Forward if needed
    }
}