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
    public float deathSequenceDuration = 2f; // The death animation lasts 2 seconds
    public int numberOfExplosions = 6; // Number ofexplosions

    private bool isDead = false;

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
        if (shieldVisual != null) shieldVisual.SetActive(true);
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
            
            // Plau sound
            // if (shieldHitSound != null) AudioSource.PlayClipAtPoint(shieldHitSound, transform.position);

            // If the shield is broken
            if (currentShieldHealth <= 0)
            {
                BreakShield();
            }
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

            // Optional : add impact or harm sound here
            if (currentHealth <= 0)
            {
                StartCoroutine(DeathSequence()); // Start the dead animation sequence
            }
        }
    }

    private void BreakShield()
    {
        isShieldActive = false;
        
        // Hide the shield visual
        if (shieldVisual != null) shieldVisual.SetActive(false);
        
        // Play the sound of breaking the shield
        PlaySoundInEars(shieldBreakSound);
        // if (shieldBreakSound != null) AudioSource.PlayClipAtPoint(shieldBreakSound, transform.position);
        
        // Start the stopwatch to regenerate it
        StartCoroutine(ShieldRegenTimer());
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
        
        if (shieldVisual != null) shieldVisual.SetActive(true);
        PlaySoundInEars(shieldRegenSound);
        // if (shieldRegenSound != null) AudioSource.PlayClipAtPoint(shieldRegenSound, transform.position);
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

        // Hide health bar
        if (healthBarCanvas != null) healthBarCanvas.SetActive(false);

        // Desactivate collider for news arrows to pass through
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // (If the boss was moving, this is where you have to disable his movement script so that it stops in the air.)
        // ex: GetComponent<BossMovement>().enabled = false;

        // Explosion loop
        float delayBetweenExplosions = deathSequenceDuration / numberOfExplosions;

        for (int i = 0; i < numberOfExplosions; i++)
        {
            // Calculate a random position around the boss center
            Vector3 randomOffset = Random.insideUnitSphere * 3f;
            Vector3 explosionPos = transform.position + randomOffset;

            // Makes the explosion particles appear
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, explosionPos, Quaternion.identity);
            }

            // Play explosion sound
            // if (explosionSound != null)
            // {
            //     AudioSource.PlayClipAtPoint(explosionSound, explosionPos);
            // }
            PlaySoundInEars(explosionSound);

            // Takes a break before the next explosion
            yield return new WaitForSeconds(delayBetweenExplosions);
        }

        // Give points
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(bossPointsValue);
        }

        // Destroy boss -> Win screen
        Destroy(gameObject);
    }

    private void PlaySoundInEars(AudioClip clip)
    {
        if (clip != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, soundVolume);
        }
    }
}