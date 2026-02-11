using UnityEngine;
using TMPro;

public class EndScreenManager : MonoBehaviour
{
    public TextMeshProUGUI gameOverText;
    public UnityEngine.UI.RawImage gameOverBackground;
    public GameObject restartButton;
    
    private bool gameOverShown = false;
    private bool gameWon = false;

    void Start()
    {
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }
        if (gameOverBackground != null)
        {
            gameOverBackground.gameObject.SetActive(false);
        }
        if (restartButton != null)
        {
            restartButton.SetActive(false);
        }
    }

    void Update()
    {
        int sheepCount = FindObjectsOfType<MoutonMovement>().Length;
        
        // Check win condition
        if (!gameWon && !gameOverShown && sheepCount > 0)
        {
            EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
            
            if (spawner != null && spawner.IsSpawningStopped() && spawner.IsBossActive())
            {
                if (!spawner.DoesBossExist())
                {
                    ShowWinScreen(sheepCount);
                }
            }
        }

        // Check lose condition
        if (sheepCount == 0 && !gameOverShown)
        {
            ShowLoseScreen();
        }
    }

    private void ShowWinScreen(int sheepCount)
    {
        gameWon = true;
        gameOverShown = true;
        
        ShowEndScreen($"<size=60>You Win !</size>\n<size=30>Score: 0\n +{sheepCount} sheep saved</size>\n<size=60>= {sheepCount}</size>", new Color32(0x62, 0x2E, 0x03, 0xFF));
    }

    private void ShowLoseScreen()
    {
        gameOverShown = true;
        
        // Stop all boss effects immediately
        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.StopBossEffects();
        }
        
        ShowEndScreen("Game Over", new Color32(0x62, 0x2E, 0x03, 0xFF));
    }

    private void ShowEndScreen(string message, Color textColor)
    {
        // Stop all audio sources in the scene
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audioSource in allAudioSources)
        {
            audioSource.Stop();
        }
        
        if (gameOverText != null)
        {
            // Position the end screen in world space in front of player (only on first show)
            Canvas canvas = gameOverText.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.WorldSpace)
            {
                Transform playerCamera = Camera.main.transform;
                if (playerCamera != null)
                {
                    canvas.renderMode = RenderMode.WorldSpace;
                    
                    // Get player forward direction but keep it horizontal (ignore vertical tilt)
                    Vector3 forward = playerCamera.forward;
                    forward.y = 0;
                    
                    // If player is looking straight up/down, use the camera's right vector instead
                    if (forward.magnitude < 0.1f)
                    {
                        forward = playerCamera.right;
                        forward.y = 0;
                    }
                    
                    forward.Normalize();
                    
                    // Position 3 meters in front of player at eye level
                    Vector3 screenPosition = playerCamera.position + forward * 3f;
                    // Use the player's current Y position for eye level
                    screenPosition.y = playerCamera.position.y;
                    
                    canvas.transform.position = screenPosition;
                    // Rotate to face the player using only horizontal rotation
                    canvas.transform.rotation = Quaternion.LookRotation(forward);
                    canvas.transform.localScale = Vector3.one * 0.003f;
                }
            }
            
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = message.ToLower();
            gameOverText.color = textColor;
            gameOverText.alignment = TextAlignmentOptions.Center;
            gameOverText.fontSize = 100;
        }
        
        if (gameOverBackground != null)
        {
            gameOverBackground.gameObject.SetActive(true);
        }
        
        if (restartButton != null)
        {
            restartButton.SetActive(true);
        }
        
        Time.timeScale = 0f;
    }
}
