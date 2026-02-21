using UnityEngine;
using UnityEngine.UI;

public class HealthBarEffect : MonoBehaviour
{
    public Image realFillImage;    // The colored bar
    public Image delayedFillImage; // The delayed bar behind
    public float delaySpeed = 2f;  // Catch-up speed

    void Update()
    {
        // If the white bar is larger than the real bar...
        if (delayedFillImage.fillAmount > realFillImage.fillAmount)
        {
            // ...she slowly reduces to catch up with her
            delayedFillImage.fillAmount -= Time.deltaTime * delaySpeed;
        }
        else
        {
            // Otherwise, she stays steady on it
            delayedFillImage.fillAmount = realFillImage.fillAmount;
        }
    }
}