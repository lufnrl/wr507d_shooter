using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    // Singleton : Allow to access the script anywhere with "ScoreManager.Instance"
    public static ScoreManager Instance;
    public TextMeshProUGUI scoreText;
    public int currentScore = 0;

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

    private void Start()
    {
        // Set the display to 0 at the launch of the game
        UpdateScoreDisplay();
    }

    public void AddScore(int pointsToAdd)
    {
        currentScore += pointsToAdd;
        Debug.Log($"CIBLE TOUCHÉE ! (+{pointsToAdd}) | Score Total : {currentScore}");

        // Update the stone with each point earned
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null) 
        {
            scoreText.text = "SCORE : " + currentScore.ToString();
        }
    }
}
