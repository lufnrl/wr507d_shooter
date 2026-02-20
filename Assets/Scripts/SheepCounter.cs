using UnityEngine;
using TMPro;

public class SheepCounter : MonoBehaviour
{
    public TextMeshProUGUI sheepLeftText;

    void Update()
    {
        int sheepCount = FindObjectsOfType<MoutonMovement>().Length;
        if (sheepLeftText != null)
        {
            sheepLeftText.text = $"{sheepCount}";
            if (sheepCount <= 3)
            {
                sheepLeftText.color = new Color32(0x8B, 0x00, 0x00, 0xFF);
            }
            else
            {
                sheepLeftText.color = new Color32(0x28, 0x28, 0x28, 0xFF);
            }
        }
    }
}
