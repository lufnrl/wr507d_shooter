using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    // Singleton : Allow to access the script anywhere with "ScoreManager.Instance"
    public static ScoreManager Instance;

    private int currentScore = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddScore(int pointsToAdd)
    {
        currentScore += pointsToAdd;
        Debug.Log($"CIBLE TOUCHÉE ! (+{pointsToAdd}) | Score Total : {currentScore}");
    }
}
