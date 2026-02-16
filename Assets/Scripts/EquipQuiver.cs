using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

[RequireComponent(typeof(XRSimpleInteractable))]
public class EquipQuiver : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private Transform quiverZone; 
    [SerializeField] private MonoBehaviour outlineScript;

    [Header("Liaison avec l'Arme")]
    [SerializeField] private ArrowController bowArrowController;

    [Header("Feedback (UI et Son)")]
    [SerializeField] private GameObject feedbackCanvas; 
    
    [Tooltip("Le texte UI à modifier")]
    [SerializeField] private TextMeshProUGUI feedbackMessage;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip equipSound;

    private XRSimpleInteractable simpleInteractable;

    void Start()
    {
        simpleInteractable = GetComponent<XRSimpleInteractable>();
        
        if (simpleInteractable != null)
        {
            simpleInteractable.selectEntered.AddListener(OnQuiverEquipped);
        }

        if (feedbackCanvas != null)
        {
            feedbackCanvas.SetActive(false);
        }
    }

    private void OnQuiverEquipped(SelectEnterEventArgs args)
    {
        // Disable outline
        if (outlineScript != null) outlineScript.enabled = false;

        // Make the controller vibrate (Haptic Feedback)
        if (args.interactorObject is XRBaseControllerInteractor controllerInteractor)
        {
            controllerInteractor.SendHapticImpulse(0.7f, 0.3f);
        }

        // Teleport and tie in the back
        if (quiverZone != null)
        {
            transform.SetParent(quiverZone);
            
            // Coordinates to zero so that it aligns perfectly with the QuiverZone
            transform.localPosition = Vector3.zero;
            
            // Ajust this rotation
            // For example, to tilt it on the shoulder: Quaternion.Euler(0, 0, 45);
            transform.localRotation = Quaternion.identity; 
        }

        // Play equipment sound
        if (audioSource != null && equipSound != null)
        {
            audioSource.PlayOneShot(equipSound);
        }

        // Display feedback message on canvas
        if (feedbackCanvas != null && feedbackMessage != null)
        {
            feedbackMessage.text = "Carquois bien équipé dans votre dos !";
            feedbackCanvas.SetActive(true);
            
            StartCoroutine(HideMessageAfterDelay(4f));
        }

        // Reload arrows
        if (bowArrowController != null)
        {
            bowArrowController.Reload();
        }

        // Disable interaction
        simpleInteractable.enabled = false;
    }

    private IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        // Turn off the Canvas after the delay
        if (feedbackCanvas != null)
        {
            feedbackCanvas.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (simpleInteractable != null)
        {
            simpleInteractable.selectEntered.RemoveListener(OnQuiverEquipped);
        }
    }
}