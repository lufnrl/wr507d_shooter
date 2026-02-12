using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingTarget : MonoBehaviour, IHittable
{
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField] private int pointsValue = 10; // Valeur par défaut, modifiable dans l'inspecteur

    // private void OnCollisionEnter(Collision collision)
    // {
    //     if ((rb.isKinematic || collision.gameObject.CompareTag("Arrow")) == false)
    //     {
    //         audioSource.Play();
    //     }
    // }

    public void GetHit()
    {
        // 1. On prévient le Manager d'ajouter les points
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(pointsValue);
        }
    }
}

public interface IHittable
{
    void GetHit();
}