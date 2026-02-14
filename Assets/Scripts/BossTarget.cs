using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Obligatoire pour parler à l'Image de la barre de vie

public class BossTarget : MonoBehaviour, IHittable
{
    [Header("Statistiques")]
    public int maxHealth = 6; // Nombre de flèches pour le tuer
    private int currentHealth;
    public int bossPointsValue = 500;

    [Header("Mécanique de Bouclier")]
    public int shieldMaxHealth = 3; // Flèches nécessaires pour casser le bouclier
    private int currentShieldHealth;
    public float vulnerableDuration = 8f; // Temps en secondes avant que le bouclier revienne
    public GameObject shieldVisual; // Glisse ici ta sphère ShieldVisual
    
    // Optionnel : Des petits sons pour bien comprendre ce qu'il se passe
    public AudioClip shieldHitSound;   // Quand on tape le bouclier
    public AudioClip shieldBreakSound; // Quand le bouclier explose (vulnérable !)
    public AudioClip shieldRegenSound; // Quand le bouclier se reforme
    
    private bool isShieldActive = true; // Le boss commence avec son bouclier allumé !

    [Header("Interface (UI)")]
    public Image healthBarFill; // Glisse ici ton image "Fill" rouge/verte
    public GameObject healthBarCanvas; // Glisse le Canvas entier ici pour le cacher à la fin

    [Header("Death effects")]
    public GameObject explosionPrefab; // Un effet de particules (optionnel)
    public AudioClip explosionSound;   // Le son d'une petite explosion
    public float deathSequenceDuration = 2f; // L'animation de mort dure 2 secondes
    public int numberOfExplosions = 6; // Il y aura 6 petites explosions

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        currentShieldHealth = shieldMaxHealth; // On charge le bouclier
        UpdateHealthBar();

        if (healthBarCanvas != null)
        {
            healthBarCanvas.SetActive(false);
        }

        // On s'assure que le visuel du bouclier est bien allumé au départ
        if (shieldVisual != null) shieldVisual.SetActive(true);
    }

    public void GetHit()
    {
        // Boss being exploded, ignore news arrows
        if (isDead) return; 

        // 1. SI LE BOUCLIER EST ACTIF
        if (isShieldActive)
        {
            currentShieldHealth--;
            
            // Joue un son de "Klang" métallique/énergétique
            if (shieldHitSound != null) AudioSource.PlayClipAtPoint(shieldHitSound, transform.position);

            // Si on a cassé le bouclier !
            if (currentShieldHealth <= 0)
            {
                BreakShield();
            }
        }
        // 2. SI LE BOUCLIER EST CASSÉ (Le Boss prend cher !)
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
        
        // On cache la bulle
        if (shieldVisual != null) shieldVisual.SetActive(false);
        
        // On joue le son de bris de glace/énergie
        if (shieldBreakSound != null) AudioSource.PlayClipAtPoint(shieldBreakSound, transform.position);
        
        // On lance le chrono pour le régénérer
        StartCoroutine(ShieldRegenTimer());
    }

    private IEnumerator ShieldRegenTimer()
    {
        // On attend la durée de vulnérabilité (ex: 8 secondes)
        yield return new WaitForSeconds(vulnerableDuration);
        
        // Si le boss a été tué pendant ce temps, on annule la régénération !
        if (isDead) yield break;

        // Le bouclier est de retour !
        isShieldActive = true;
        currentShieldHealth = shieldMaxHealth; // On lui remet ses points de vie
        
        if (shieldVisual != null) shieldVisual.SetActive(true);
        if (shieldRegenSound != null) AudioSource.PlayClipAtPoint(shieldRegenSound, transform.position);
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

        // (Si ton boss bougeait, c'est ici qu'il faut désactiver son script de mouvement pour qu'il s'arrête en l'air)
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
            if (explosionSound != null)
            {
                AudioSource.PlayClipAtPoint(explosionSound, explosionPos);
            }

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
}