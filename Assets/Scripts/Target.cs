using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour, IHittable
{
    [Header("Réglages")]
    [SerializeField] private int pointsValue = 10; 
    [SerializeField] private float timeBeforeDestroy = 0.15f;

    [Header("Effets")]
    // On demande directement le fichier son, plus besoin du composant AudioSource !
    [SerializeField] private AudioClip destructionSound; 
    [SerializeField] [Range(0f, 1f)] private float soundVolume = 1.0f;

    public void GetHit()
    {
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

        // Destroy ufo and sticky arrow child
        // Debug.Log($"Target hit! +{pointsValue} points");
        Destroy(gameObject, timeBeforeDestroy);
    }
}

public interface IHittable
{
    void GetHit();
}