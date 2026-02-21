using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit; // Necessary to detect hands

public class ArrowSideSwitcher : MonoBehaviour
{
    public string rightHandTag = "RightHand";
    public Transform arrow; // The arrow visible on the rope before the shot (contains the spawnPoint)

    public void UpdateArrowSide(SelectEnterEventArgs args)
    {
        // Identify who caught the bow (the "Interactor")
        Transform handTransform = args.interactorObject.transform; // args.interactorObject is the hand. We look at his Transform.
        
        // Check the tag on the main object OR on its parent (the Controller)
        bool isRightHand = handTransform.CompareTag(rightHandTag) || 
                (handTransform.parent != null && handTransform.parent.CompareTag(rightHandTag));

        if (arrow != null)
        {
            Vector3 pos = arrow.localPosition;

            // Define the factor: 1 for Right-handed, -1 for Left-handed
            float sideFactor = isRightHand ? 1f : -1f;

            // Take the absolute value and we apply the sign
            pos.x = Mathf.Abs(pos.x) * sideFactor;
            arrow.localPosition = pos;
        }
    }
    
}

