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
        // On initialise l'affichage à 0 au lancement du jeu
        UpdateScoreDisplay();
    }

    public void AddScore(int pointsToAdd)
    {
        currentScore += pointsToAdd;
        Debug.Log($"CIBLE TOUCHÉE ! (+{pointsToAdd}) | Score Total : {currentScore}");

        // On met à jour la pierre à chaque point gagné !
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        // On vérifie que la case n'est pas vide pour éviter les erreurs
        if (scoreText != null) 
        {
            scoreText.text = "SCORE : " + currentScore.ToString();
        }
    }
}
