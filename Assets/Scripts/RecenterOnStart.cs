using System.Collections;
using UnityEngine;
using Unity.XR.CoreUtils;

public class RecenterOnStart : MonoBehaviour
{
    private XROrigin xrOrigin;

    void Start()
    {
        xrOrigin = GetComponent<XROrigin>();
        
        // On attend une toute petite seconde que le casque VR s'allume et s'initialise bien
        StartCoroutine(RecenterRoutine());
    }

    private IEnumerator RecenterRoutine()
    {
        yield return new WaitForSeconds(0.5f); 
        
        if (xrOrigin != null)
        {
            // Force la caméra (le joueur) à regarder dans la même direction que la flèche bleue (Z) de ton XR Origin
            xrOrigin.MatchOriginUpCameraForward(xrOrigin.transform.up, xrOrigin.transform.forward);
        }
    }
}