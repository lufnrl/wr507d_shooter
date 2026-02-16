using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DisableOutlineOnGrab : MonoBehaviour
{
    [Header("Composants")]
    [Tooltip("L'Interactable de l'arc (généralement sur le même objet)")]
    [SerializeField] private XRGrabInteractable grabInteractable;
    
    [Tooltip("Le script Outline à désactiver")]
    [SerializeField] private MonoBehaviour outlineScript; 

    void Start()
    {
        // If we haven’t manually dragged the component, we look for it on the object
        if (grabInteractable == null)
        {
            grabInteractable = GetComponent<XRGrabInteractable>();
        }

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnFirstGrab);
        }
    }

    private void OnFirstGrab(SelectEnterEventArgs args)
    {
        // Disable the Outline script
        if (outlineScript != null)
        {
            outlineScript.enabled = false;
        }

        // Uunsubscribe from the event so that this code runs only once
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnFirstGrab);
        }
    }

    void OnDestroy()
    {
        // Usual security in Unity when using Listeners
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnFirstGrab);
        }
    }
}