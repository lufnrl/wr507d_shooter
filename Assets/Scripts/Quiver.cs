using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Quiver : MonoBehaviour
{
    public ArrowController arrowController; // Référence à ton script de gestion
    public AudioSource audioSource;
    public AudioClip reloadSound;

    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log("OnTriggerEnter carquois détecté avec : " + other.name);

        // Verify that it's a hand which is trigger in the zone of the quiver
        // We check both the hand and its parent (the controller) for the tag
        if (other.CompareTag("RightHand") 
            || (other.transform.parent != null && other.transform.parent.CompareTag("RightHand"))
            || other.CompareTag("LeftHand")
            || (other.transform.parent != null && other.transform.parent.CompareTag("LeftHand")))
        {
            // Reload by contact for simplicity
            ReloadArrows();
        }
    }

    private void ReloadArrows()
    {
        // Reload only if the quiver is not already full
        if (!arrowController.IsQuiverFull())
        {
            arrowController.Reload(); // Call the Reload function of ArrowController
            
            // Sound to confirm
            if (audioSource != null && reloadSound != null)
            {
                audioSource.PlayOneShot(reloadSound);
            }

            // Debug.Log("Carquois rechargé !");
        }
        else
        {
            // Debug.Log("Carquois plein !");
        }
    }
}