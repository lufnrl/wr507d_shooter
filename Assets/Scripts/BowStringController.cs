using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class BowStringController : MonoBehaviour
{
    [SerializeField]
    private BowString bowStringRenderer;

    private XRGrabInteractable interactable;

    [SerializeField]
    private Transform midPointGrabObject, midPointVisualObject, midPointParent;


    [SerializeField]
    private float bowStringStretchLimit = 0.28f;

    private Transform interactor;

    private float strength, previousStrength;

    [SerializeField]
    private float stringSoundThreshold = 0.001f;

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField] private ArrowController arrowController;
    [SerializeField] private GameObject arrowVisualMesh;

    public UnityEvent OnBowPulled; // Allow string visualization
    public UnityEvent<float> OnBowReleased; // The moment we shoot



    private void Awake()
    {
        // Grabbing the cube on the string
        interactable = midPointGrabObject.GetComponent<XRGrabInteractable>();
    }

    private void Start()
    {
        // Press the grip trigger
        interactable.selectEntered.AddListener(PrepareBowString);
        interactable.selectExited.AddListener(ResetBowString);
    }

    private void ResetBowString(SelectExitEventArgs arg0)
    {
        if (arrowController != null && arrowController.HasArrows() && strength > 0.1f) // Little dead zone
        {
            OnBowReleased?.Invoke(strength); // Call ReleaseArrow in another script
        }
        else
        {
            // Debug.Log("Pas de flèche ou pas assez de force.");
        }

        strength = 0;
        previousStrength = 0;
        audioSource.pitch = 1;
        audioSource.Stop();

        interactor = null;
        midPointGrabObject.localPosition = Vector3.zero;
        midPointVisualObject.localPosition = Vector3.zero;
        bowStringRenderer.CreateString(null); // Recreate the string straight

        // Cache visual arrow
        if (arrowVisualMesh != null) arrowVisualMesh.SetActive(false);
    }

    private void PrepareBowString(SelectEnterEventArgs arg0)
    {
        interactor = arg0.interactorObject.transform;

        if (arrowController != null && arrowController.HasArrows())
        {
            OnBowPulled?.Invoke();  // Make appear the string visualization
            if (arrowVisualMesh != null) arrowVisualMesh.SetActive(true);
        }
        else
        {
            // If no arrow, cache visual but the string can be pulled
            if (arrowVisualMesh != null) arrowVisualMesh.SetActive(false);
            // Debug.Log("Carquois VIDE : Pas de flèche visuelle !");
        }
    }


    private void Update()
    {
        if (interactor != null)
        {
            // Convert bow string mid point position to the local space of the MidPoint
            Vector3 midPointLocalSpace = 
                midPointParent.InverseTransformPoint(midPointGrabObject.position); // can also take localPosition

            //Get the offset
            float midPointLocalZAbs = Mathf.Abs(midPointLocalSpace.z);

            previousStrength = strength;

            HandleStringPushedBackToStart(midPointLocalSpace); // String be straight when not pulled

            HandleStringPulledBackTolimit(midPointLocalZAbs, midPointLocalSpace); // Constrain the string pull limit

            HandlePullingString(midPointLocalZAbs, midPointLocalSpace); // Pulling the string within zero and limit

            bowStringRenderer.CreateString(midPointVisualObject.position); // Recreate the string with the new mid point position
        }
    }

    private void HandlePullingString(float midPointLocalZAbs, Vector3 midPointLocalSpace)
    {
        // what happens when we are between point 0 and the string pull limit
        if (midPointLocalSpace.z < 0 && midPointLocalZAbs < bowStringStretchLimit)
        {
            if (audioSource.isPlaying == false && strength <= 0.01f)
            {
                audioSource.Play();
            }

            strength = Remap(midPointLocalZAbs, 0, bowStringStretchLimit, 0, 1);
            midPointVisualObject.localPosition = new Vector3(0, 0, midPointLocalSpace.z); // Dont move on x or y axis

            PlayStringPullinSound();
        }
    }

    private void PlayStringPullinSound()
    {
        // Check if we have moved the string enought to play the sound unpause it
        if (Mathf.Abs(strength - previousStrength) > stringSoundThreshold)
        {
            if (strength < previousStrength)
            {
                // Play string sound in reverse if we are pusing the string towards the bow
                audioSource.pitch = -1;
            }
            else
            {
                // Play the sound normally
                audioSource.pitch = 1;
            }
            audioSource.UnPause();
        }
        else
        {
            // If we stop moving Pause the sounds
            audioSource.Pause();
        }

    }

    private float Remap(float value, int fromMin, float fromMax, int toMin, int toMax)
    {
        return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin; // Recalculate the value
    }


    private void HandleStringPulledBackTolimit(float midPointLocalZAbs, Vector3 midPointLocalSpace)
    {
        // We specify max pulling limit for the string. We don't allow the string to go any farther than "bowStringStretchLimit"
        if (midPointLocalSpace.z < 0 && midPointLocalZAbs >= bowStringStretchLimit)
        {
            audioSource.Pause();
            strength = 1; // Max strength
            midPointVisualObject.localPosition = new Vector3(0, 0, -bowStringStretchLimit);
        }
    }

    private void HandleStringPushedBackToStart(Vector3 midPointLocalSpace)
    {
        if (midPointLocalSpace.z >= 0)
        {
            audioSource.pitch = 1;
            audioSource.Stop();
            strength = 0;
            midPointVisualObject.localPosition = Vector3.zero; // Reset to original position
        }
    }
}
