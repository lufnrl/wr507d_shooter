using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

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

    // Pour savoir combien on en a tiré au total (pour les stats de fin de jeu par exemple)
    // private int totalArrowsFired = 0;

    private void Start()
    {
        // Fill the quiver at the start
        currentArrowCount = maxArrows;
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
            // totalArrowsFired++;

            // Debug.Log($"Flèche tirée ! Restantes : {currentArrowCount} / Total tiré : {totalArrowsFired}");
            Debug.Log($"Flèche tirée ! Restantes : {currentArrowCount}");
        } 
        else
        {
            Debug.Log("CLIC ! Plus de flèches ! Il faut recharger.");
            // Ici, tu pourras ajouter un petit son "Clic" (arme vide) plus tard
        }
        
    }

    // Reload arrows in quiver
    public void Reload()
    {
        currentArrowCount = maxArrows;
        Debug.Log("Rechargement effectué !");
    }
}