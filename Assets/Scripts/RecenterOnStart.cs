using System.Collections;
using UnityEngine;
using Unity.XR.CoreUtils;

public class RecenterOnStart : MonoBehaviour
{
    private XROrigin xrOrigin;

    void Start()
    {
        xrOrigin = GetComponent<XROrigin>();
        
        // Wait a second for the VR headset to turn on and initialize properly
        StartCoroutine(RecenterRoutine());
    }

    private IEnumerator RecenterRoutine()
    {
        yield return new WaitForSeconds(0.5f); 
        
        if (xrOrigin != null)
        {
            // Force the camera (the player) to look in the same direction as the blue arrow (Z) of your XR Origin
            xrOrigin.MatchOriginUpCameraForward(xrOrigin.transform.up, xrOrigin.transform.forward);
        }
    }
}