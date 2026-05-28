using UnityEngine;
using UnityEngine.EventSystems;

public class RunButtonTest : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public PlayerMovement playerMovement; // Drag Capsule here

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("BUTTON PRESSED - OnPointerDown called!");
        
        if (playerMovement != null)
        {
            Debug.Log("Calling StartRunning on PlayerMovement");
            playerMovement.StartRunning();
        }
        else
        {
            Debug.LogError("PlayerMovement reference is missing! Drag the Capsule into the field.");
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("BUTTON RELEASED - OnPointerUp called!");
        
        if (playerMovement != null)
        {
            Debug.Log("Calling StopRunning on PlayerMovement");
            playerMovement.StopRunning();
        }
    }
}