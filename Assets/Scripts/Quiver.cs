using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Quiver : MonoBehaviour
{
    public ArrowController arrowController; // Référence à ton script de gestion
    public AudioSource audioSource;
    public AudioClip reloadSound;


    // Function to make vibrate feedback when player enters the quiver zone
    private void TriggerHaptic(Collider handCollider)
    {
        // Get controller form the hand collider
        var controller = handCollider.GetComponentInParent<ActionBasedController>();
        
        if (controller != null)
        {
            // Intensity (0 to 1) and last (in secondes)
            controller.SendHapticImpulse(0.5f, 0.1f); 
        }
    }

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

            // Vibrate to say "Zone trouvée !"
            TriggerHaptic(other);

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