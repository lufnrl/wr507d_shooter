using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Mandatory to speak to the Image of the life bar

public class BossTarget : MonoBehaviour, IHittable
{
    [Header("Statistiques")]
    public int maxHealth = 6; // Number of arrows to kill him
    private int currentHealth;
    public int bossPointsValue = 500;

    [Header("Mécanique de Bouclier")]
    public int shieldMaxHealth = 3; // Arrows needed to break the shield
    private int currentShieldHealth;
    public float vulnerableDuration = 8f; // Time in seconds before the shield returns
    public GameObject shieldVisual;

    [Header("Feedback Visuel Bouclier")] // --- NOUVEAU ---
    public Color damageFlashColor = Color.white; // Rougeâtre par défaut
    public float flashDuration = 0.1f; // Durée du flash
    private Renderer shieldRenderer; // Pour changer la couleur
    public Color healthyColor = new Color(0f, 1f, 0f, 0.8f); // Vert (Pleinne vie)
    public Color brokenColor = new Color(1f, 0f, 0f, 0.8f);  // Rouge (Presque mort)
    
    [Header("Paramètres Audio")]
    [Range(0f, 1f)] 
    public float soundVolume = 0.5f;
    public AudioClip shieldHitSound;   // When we tap the shield
    public AudioClip shieldBreakSound; // When the shield explodes
    public AudioClip shieldRegenSound; // When the shield is reformed
    
    private bool isShieldActive = true; // The boss starts with his shield on

    [Header("Interface (UI)")]
    public Image healthBarFill;
    public GameObject healthBarCanvas;

    [Header("Death effects")]
    public GameObject explosionPrefab; // A particle effect
    public AudioClip explosionSound;   // The sound of an explosion
    public float deathSequenceDuration = 6f; // The death animation lasts 2 seconds
    public int numberOfExplosions = 6; // Number of explosions
    public GameObject finalSmokePrefab;

    public bool isDead = false;
    public bool isCrashFinished = false;


    void Start()
    {
        currentHealth = maxHealth;
        currentShieldHealth = shieldMaxHealth; // Charge the shield
        UpdateHealthBar();

        if (healthBarCanvas != null)
        {
            healthBarCanvas.SetActive(false);
        }

        // Make sure that the shield visual is well lit at the start
        if (shieldVisual != null) 
        {
            shieldVisual.SetActive(true);
            shieldRenderer = shieldVisual.GetComponent<Renderer>();
            
            // On sauvegarde la couleur initiale pour pouvoir y revenir après un flash
            if (shieldRenderer != null)
            {
                shieldRenderer.material.color = healthyColor;
            }
        }

        
        // if (shieldVisual != null) shieldVisual.SetActive(true);
    }

    public void GetHit()
    {
        // Boss being exploded, ignore news arrows
        if (isDead) return; 

        // If the shiled is active
        if (isShieldActive)
        {
            currentShieldHealth--;
            PlaySoundInEars(shieldHitSound);

            if (shieldRenderer != null)
            {
                StartCoroutine(FlashShield());
            }
    
            // If the shield is broken
            if (currentShieldHealth <= 0)
            {
                BreakShield();
            }
            return;
        }
        // If the shield is broken
        else
        {
            // Show health bar at first hit
            if (healthBarCanvas != null && healthBarCanvas.activeSelf == false)
            {
                healthBarCanvas.SetActive(true);
            }

            currentHealth--;
            UpdateHealthBar();

            if (currentHealth <= 0)
            {
                StartCoroutine(DeathSequence()); // Start the dead animation sequence
            }
        }
    }

    private IEnumerator FlashShield()
    {
        // 1. FLASH D'IMPACT (Blanc/Rouge vif)
        shieldRenderer.material.color = damageFlashColor;

        // Attente du flash
        yield return new WaitForSeconds(flashDuration);

        // 2. CALCUL SÉCURISÉ
        // On s'assure que le résultat est entre 0.0 et 1.0
        float healthPercentage = (float)currentShieldHealth / (float)shieldMaxHealth;
        
        // Petite sécurité : Si on est à 0 PV (le coup fatal), on force 0
        if (currentShieldHealth <= 0) healthPercentage = 0f;

        // 3. APPLICATION DE LA COULEUR
        // Lerp(Rouge, Vert, Pourcentage)
        // 1.0 = Vert (Healthy)
        // 0.0 = Rouge (Broken)
        Color targetColor = Color.Lerp(brokenColor, healthyColor, healthPercentage);

        // On force l'alpha à rester visible (au cas où tes couleurs ont de la transparence)
        targetColor.a = 0.8f; // Ou healthyColor.a

        shieldRenderer.material.color = targetColor;
    }

    private void BreakShield()
    {
        isShieldActive = false;

        if (shieldVisual != null) shieldVisual.SetActive(false); // Hide the shield visual
        PlaySoundInEars(shieldBreakSound); // Play the sound of breaking the shield
        StartCoroutine(ShieldRegenTimer()); // Start the stopwatch to regenerate it
    }

    private IEnumerator ShieldRegenTimer()
    {
        // Wait for the duration of vulnerability
        yield return new WaitForSeconds(vulnerableDuration);
        
        // If the boss was killed during this time, we cancel the regeneration
        if (isDead) yield break;

        // Shield is back
        isShieldActive = true;
        currentShieldHealth = shieldMaxHealth; // Put back his health points
        
        if (shieldVisual != null) 
        {
            shieldVisual.SetActive(true);
            if (shieldRenderer != null)
            {
                // RESET : Le bouclier revient tout neuf (Vert)
                shieldRenderer.material.color = healthyColor;
            }
        }
        // if (shieldVisual != null) shieldVisual.SetActive(true);
        PlaySoundInEars(shieldRegenSound);
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            // Transform health in percentage (ex: 3/5 = 0.6)
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    private IEnumerator DeathSequence()
    {
        isDead = true;

        // Hide health bar and interface
        if (healthBarCanvas != null) healthBarCanvas.SetActive(false);
        if (shieldVisual != null) shieldVisual.SetActive(false);

        // Cut all sounds
        AudioSource[] allAudios = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audio in allAudios) audio.Stop();

        // Turn on the sun light et dissipate the dark sky
        Light[] allLights = FindObjectsOfType<Light>();
        foreach (Light l in allLights)
        {
            if (l.type == LightType.Directional) l.intensity = 1f; // Sun at the max
        }
        RenderSettings.ambientIntensity = 1.5f;
        if (RenderSettings.skybox != null) RenderSettings.skybox.SetFloat("_Exposure", 1.1f);

        // Force EnemySpawner to stop boss movement
        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null) spawner.StopAllCoroutines();  // Stop abduction and rotation

        // Activate gravity to make the boss crash
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            rb.velocity = Vector3.zero;

            rb.drag = 0.1f; 
            rb.angularDrag = 2f;

            // Calculate the direction "away from the player" (towards the bottom of the map)
            Vector3 crashDirection = transform.forward;
            if (Camera.main != null)
            {
                crashDirection = (transform.position - Camera.main.transform.position).normalized;
            }
            crashDirection.y = -1f;

            rb.AddForce(crashDirection * 30f, ForceMode.VelocityChange);

            float spinForce = 4f;
            rb.AddTorque(new Vector3(Random.Range(-spinForce, spinForce), Random.Range(-spinForce, spinForce), Random.Range(-spinForce, spinForce)), ForceMode.VelocityChange);
        }


        // Smoke start at the beginning of the crash
        if (finalSmokePrefab != null)
        {
            Instantiate(finalSmokePrefab, transform.position, Quaternion.identity, transform);
        }

        // Explosion loop
        float delayBetweenExplosions = deathSequenceDuration / numberOfExplosions;

        for (int i = 0; i < numberOfExplosions; i++)
        {
            // Calculate a random position around the boss center (in meters)
            Vector3 randomOffset = Random.onUnitSphere * 40f;
            Vector3 explosionPos = transform.position + randomOffset;

            // Makes the explosion particles appear
            if (explosionPrefab != null)
            {
                GameObject exp = Instantiate(explosionPrefab, explosionPos, Quaternion.identity);
                exp.transform.localScale = new Vector3(15f, 15f, 15f); // Magnify the explosion x4 to see it well
            }

            PlaySoundInEars(explosionSound, 0.3f);
            yield return new WaitForSeconds(delayBetweenExplosions); // Take a break before the next explosion
        }

        // Final scene : A giant explosion just before disappearing
        if (explosionPrefab != null)
        {
            GameObject finalExp = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            finalExp.transform.localScale = new Vector3(60f, 60f, 60f); // Big BOOM
        }

        // One last sound to complete the sequence
        PlaySoundInEars(explosionSound, 1f);
        if (ScoreManager.Instance != null) ScoreManager.Instance.AddScore(bossPointsValue); // Give points

        // Make the carcass black/burned
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach(Renderer r in renderers)
        {
            if (r.material.HasProperty("_Color"))
            {
                r.material.color = new Color(0.3f, 0.3f, 0.3f); // Very dark grey (burnt)
            }
        }

        // The crash is 100% over, we authorize the victory screen
        isCrashFinished = true;
    }

    private void PlaySoundInEars(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, soundVolume * volumeMultiplier);
        }
    }
}