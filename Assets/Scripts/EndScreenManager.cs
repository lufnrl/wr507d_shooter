using UnityEngine;
using TMPro;

public class EndScreenManager : MonoBehaviour
{
    public Canvas endScreenCanvas;
    public TextMeshProUGUI gameOverText;
    public UnityEngine.UI.RawImage gameOverBackground;
    public GameObject restartButton;
    [SerializeField] public int sheepBonusValue = 100;
    
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
        // On désactive tout le Canvas au départ, c'est plus propre
        if (endScreenCanvas != null)
        {
            endScreenCanvas.gameObject.SetActive(false);
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

        int baseScore = 0;
        if (ScoreManager.Instance != null)
        {
            baseScore = ScoreManager.Instance.currentScore;
        }

        int sheepBonus = sheepCount * sheepBonusValue; // Calculate bonus
        int finalTotalScore = baseScore + sheepBonus; // Calculate total
        
        // Inject in text with formatage
        ShowEndScreen(
            $"<size=60>You Win !</size>\n" +
            $"<size=30>Score : {baseScore}\n" +
            $"+{sheepCount} sheep saved ({sheepBonus} pts)</size>\n" +
            $"<size=60>Total = {finalTotalScore}</size>", 
            new Color32(0x62, 0x2E, 0x03, 0xFF)
        );
    }

    private void ShowLoseScreen()
    {
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
        gameOverShown = true;

        // Stop all audio sources in the scene
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audioSource in allAudioSources)
        {
            audioSource.Stop();
        }

        // Freeze all enemies and spawner
        FreezeGame();

        if (endScreenCanvas != null)
        {
            if (Camera.main != null)
            {
                Transform playerCamera = Camera.main.transform;
                
                Vector3 forward = playerCamera.forward;
                forward.y = 0;
                forward.Normalize();
                
                Vector3 screenPosition = playerCamera.position + forward * 3f;
                screenPosition.y = playerCamera.position.y;
                
                endScreenCanvas.transform.position = screenPosition;
                endScreenCanvas.transform.rotation = Quaternion.LookRotation(forward);

                endScreenCanvas.transform.localScale = new Vector3(0.003f, 0.003f, 0.003f);
                // Debug.LogWarning("CAMÉRA TROUVEE !");
            }
            // else
            // {
            //     Debug.LogWarning("CAMÉRA INTROUVABLE !");
            // }
            
            // Activate Canvas
            endScreenCanvas.gameObject.SetActive(true);
        }
        
        if (gameOverText != null)
        {
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
    }

    private void FreezeGame()
    {
        // Avoid new enemy spawn
        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.enabled = false; 
        }

        // Find all enemies and sheeps already in the scene
        MoutonMovement[] tousLesMoutons = FindObjectsOfType<MoutonMovement>();
        
        foreach (MoutonMovement mouton in tousLesMoutons)
        {
            // Desactivate sheep script
            mouton.enabled = false;

            // IF sheeps use a NavMeshAgent (intelligent movement) :
            UnityEngine.AI.NavMeshAgent agent = mouton.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.isStopped = true;
            }

            // IF sheeps use physic (Rigidbody) :
            Rigidbody rb = mouton.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero; // On annule sa vitesse
                rb.isKinematic = true;      // On le fige dans les airs
            }

            // Freeze animation (optional)
            Animator anim = mouton.GetComponent<Animator>();
            if (anim != null)
            {
                anim.speed = 0f; // Pause the animation
            }
        }
    }
}
