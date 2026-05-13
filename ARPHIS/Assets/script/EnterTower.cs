using UnityEngine;

public class EnterTower : MonoBehaviour
{
    // I-drag dito yung 'InsidePoint' sa Inspector mamaya
    public Transform targetPoint; 

    private void OnTriggerEnter(Collider other)
    {
        // Tinitingnan kung "Player" ang pumasok sa pinto
        if (other.CompareTag("Player"))
        {
            // Kung may CharacterController ang player mo (karaniwan sa 3rd person)
            CharacterController controller = other.GetComponent<CharacterController>();

            if (controller != null)
            {
                // Kailangan i-disable saglit ang controller para makapag-teleport
                controller.enabled = false;
                other.transform.position = targetPoint.position;
                controller.enabled = true;
            }
            else
            {
                // Kung normal na transform lang
                other.transform.position = targetPoint.position;
            }
            
            Debug.Log("Pumasok na ang Player sa loob!");
        }
    }
}