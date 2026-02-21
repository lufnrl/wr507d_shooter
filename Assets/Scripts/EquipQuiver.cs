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

    [Header("Liaison avec le Spawner")]
    [SerializeField] private EnemySpawner enemySpawner;

    [Header("Feedback (UI et Son)")]
    [SerializeField] private GameObject feedbackCanvas; 
    
    [Tooltip("Le texte UI à modifier")]
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

    void Update()
    {
        // Keep feedback canvas fixed in front of player's head while active
        if (feedbackCanvas != null && feedbackCanvas.activeSelf)
        {
            Transform head = Camera.main.transform;
            
            // Position 2 meters in front of player's face
            feedbackCanvas.transform.position = head.position + (head.forward * 2f);
            
            // Make it face the player
            feedbackCanvas.transform.LookAt(head);
            feedbackCanvas.transform.Rotate(0, 180, 0);
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

        // Display feedback message on canvas AFTER the countdown finishes (5 seconds delay)
        if (feedbackCanvas != null)
        {
            StartCoroutine(ShowMessageAfterCountdown(5f));
        }

        // Reload arrows
        if (bowArrowController != null)
        {
            bowArrowController.Reload();
        }

        // Warn the Spawner
        if (enemySpawner != null)
        {
            enemySpawner.PlayerEquippedQuiver();
        }

        // Disable interaction
        simpleInteractable.enabled = false;
    }

    private IEnumerator ShowMessageAfterCountdown(float delayBeforeShow)
    {
        // Wait for countdown to finish
        yield return new WaitForSeconds(delayBeforeShow);
        
        // Now show the feedback - stays visible until first arrow is shot
        if (feedbackCanvas != null)
        {
            feedbackCanvas.SetActive(true);
            
            // Tell the bow to hide this message when first arrow is shot
            if (bowArrowController != null)
            {
                bowArrowController.SetQuiverFeedbackCanvas(feedbackCanvas);
            }
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