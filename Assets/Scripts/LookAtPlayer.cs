using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main != null)
        {
            // Rotate the Canvas so that it is always facing the VR headset
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward, 
                             Camera.main.transform.rotation * Vector3.up);
        }
    }
}