using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour, IHittable
{
    [Header("Réglages")]
    [SerializeField] private int pointsValue = 10; 
    [SerializeField] private float timeBeforeDestroy = 0.15f;

    [Header("Effets")]
    [SerializeField] private AudioClip destructionSound; 
    [SerializeField] [Range(0f, 1f)] private float soundVolume = 1f;

    [SerializeField] private GameObject explosionPrefab; 
    [SerializeField] private float explosionScale = 0.5f;

    private bool isAlreadyHit = false; // Security to prevent a 2nd arrow from triggering the explosion twice

    public void GetHit()
    {
        if (isAlreadyHit) return;
        isAlreadyHit = true;

        // Say to manager to add points
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(pointsValue);
        }

        // Impact Sound
        if (destructionSound != null)
        {
            AudioSource.PlayClipAtPoint(destructionSound, transform.position, soundVolume);
        }

        if (explosionPrefab != null)
        {
            // On fait apparaître l'explosion à la position exacte de la soucoupe
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            
            // On ajuste sa taille si besoin (pratique si tu utilises la même explosion que le boss mais en plus petit)
            explosion.transform.localScale = new Vector3(explosionScale, explosionScale, explosionScale);
            
            // On détruit l'objet d'explosion après 2 secondes pour ne pas polluer la mémoire du jeu
            Destroy(explosion, 2f);
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = false;
        }

        // Destroy ufo and sticky arrow child
        // Debug.Log($"Target hit! +{pointsValue} points");
        Destroy(gameObject, timeBeforeDestroy);
    }
}

public interface IHittable
{
    void GetHit();
}