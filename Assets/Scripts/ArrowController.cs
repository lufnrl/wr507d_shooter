using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class ArrowController : MonoBehaviour
{
     [SerializeField]
    private GameObject midPointVisual, arrowPrefab, arrowSpawnPoint;

    [SerializeField]
    private float arrowMaxSpeed = 10;

    [SerializeField]
    private AudioSource bowReleaseAudioSource;

    public int maxArrows = 5;       // Capacity of the quiver
    private int currentArrowCount;   // Remaining arrows

    public TextMeshPro arrowsNumberText;

    // To know how many shooted arrows in global (for stats at the end of a game for example)
    // private int totalArrowsFired = 0;

    private void Start()
    {
        // Fill the quiver at the start
        currentArrowCount = maxArrows;
        UpdateArrowsNumberDisplay(); // To display the initial arrow count
    }

    public void PrepareArrow()
    {
        midPointVisual.SetActive(true);
    }

    public void ReleaseArrow(float strength)
    {
        // If we have arrows left in the quiver
        if (currentArrowCount > 0)
        {
            bowReleaseAudioSource.Play();
            midPointVisual.SetActive(false);
            // Debug.Log($"Bow strength is {strength}");

            GameObject arrow = Instantiate(arrowPrefab);
            arrow.transform.position = arrowSpawnPoint.transform.position;
            arrow.transform.rotation = midPointVisual.transform.rotation;
            Rigidbody rb = arrow.GetComponent<Rigidbody>();
            rb.AddForce(midPointVisual.transform.forward * strength * arrowMaxSpeed, ForceMode.Impulse);

            currentArrowCount--;
            UpdateArrowsNumberDisplay();
            // totalArrowsFired++;

        } 
        else
        {
            Debug.Log("Plus de flèches ! Il faut recharger.");
            // Here, add sound for empty state
        }
        
    }

    void UpdateArrowsNumberDisplay()
    {
        Debug.Log($"Munitions : {currentArrowCount} / {maxArrows}");

        if(arrowsNumberText != null)
        {
            arrowsNumberText.text = $"{currentArrowCount}";
            
            // Change color if empty
            if (currentArrowCount == 0) 
                arrowsNumberText.color = Color.red;
            else 
                arrowsNumberText.color = Color.white;
        }
    }

    public bool HasArrows()
    {
        return currentArrowCount > 0;
    }

    public bool IsQuiverFull()
    {
        return currentArrowCount >= maxArrows;
    }

    // Reload arrows in quiver
    public void Reload()
    {
        currentArrowCount = maxArrows;
        Debug.Log("Rechargement effectué !");
    }
}