using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Obligatoire pour parler à l'Image de la barre de vie

public class BossTarget : MonoBehaviour, IHittable
{
    [Header("Statistiques")]
    public int maxHealth = 6; // Nombre de flèches pour le tuer
    private int currentHealth;
    public int bossPointsValue = 500;

    [Header("Interface (UI)")]
    public Image healthBarFill; // Glisse ici ton image "Fill" rouge/verte
    public GameObject healthBarCanvas; // Glisse le Canvas entier ici pour le cacher à la fin

    [Header("Effets de mort")]
    public GameObject explosionPrefab; // Un effet de particules (optionnel)
    public AudioClip explosionSound;   // Le son d'une petite explosion
    public float deathSequenceDuration = 2f; // L'animation de mort dure 2 secondes
    public int numberOfExplosions = 6; // Il y aura 6 petites explosions

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void GetHit()
    {
        // Si le boss est déjà en train d'exploser, on ignore les flèches supplémentaires
        if (isDead) return; 

        currentHealth--;
        UpdateHealthBar();

        // Optionnel : Tu pourrais jouer un petit son de "Dégât" ici !

        if (currentHealth <= 0)
        {
            // On lance la séquence cinématique de mort !
            StartCoroutine(DeathSequence());
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            // Transforme la vie en pourcentage (ex: 3/5 = 0.6)
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    private IEnumerator DeathSequence()
    {
        isDead = true;

        // 1. On cache la barre de vie
        if (healthBarCanvas != null) healthBarCanvas.SetActive(false);

        // 2. On désactive le Collider pour que les nouvelles flèches passent au travers
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // (Si ton boss bougeait, c'est ici qu'il faut désactiver son script de mouvement pour qu'il s'arrête en l'air)
        // ex: GetComponent<BossMovement>().enabled = false;

        // 3. La boucle des explosions 💥
        float delayBetweenExplosions = deathSequenceDuration / numberOfExplosions;

        for (int i = 0; i < numberOfExplosions; i++)
        {
            // Calcule une position aléatoire autour du centre du boss
            Vector3 randomOffset = Random.insideUnitSphere * 3f; // Ajuste le "3f" selon la taille physique de ta soucoupe
            Vector3 explosionPos = transform.position + randomOffset;

            // Fait apparaître les particules d'explosion
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, explosionPos, Quaternion.identity);
            }

            // Joue le son d'explosion
            if (explosionSound != null)
            {
                AudioSource.PlayClipAtPoint(explosionSound, explosionPos);
            }

            // Fait une pause avant la prochaine explosion
            yield return new WaitForSeconds(delayBetweenExplosions);
        }

        // 4. Donner les points au joueur
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(bossPointsValue);
        }

        // 5. On détruit enfin le boss (Ce qui va déclencher ton écran "You Win !")
        Destroy(gameObject);
    }
}