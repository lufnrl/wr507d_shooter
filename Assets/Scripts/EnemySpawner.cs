using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject[] ovniPrefabs; // Array of 3 different OVNI types

    [SerializeField]
    private GameObject moutonPrefab;

    [Header("Final Boss")]
    [SerializeField]
    private GameObject finalBossPrefab; // OVNI_3_final
    
    [SerializeField]
    private float bossRiseSpeed = 10f; // Speed at which boss rises and moves
    
    [SerializeField]
    private float bossRiseHeight = 15f; // How high the boss rises (reduced from 30)
    
    [SerializeField]
    private float bossDistanceFromPlayer = 10f; // Distance in front of player where boss positions itself
    
    [SerializeField]
    private float bossAbductionDelay = 2f; // Delay before boss starts abducting sheep
    
    [SerializeField]
    private float bossAbductionDuration = 5f; // How long boss takes to abduct all sheep
    
    [SerializeField]
    private float sheepRiseSpeed = 3f; // Speed at which sheep rise toward boss
    
    [SerializeField]
    private float cameraShakeIntensity = 0.3f; // How much the XR Rig shakes (position in meters)
    
    [SerializeField]
    private float cameraShakeDuration = 3f; // How long the shake lasts
    
    [SerializeField]
    private GameObject bowObject; // Reference to the bow to keep it stable during shake
    
    [SerializeField]
    private float lightDimAmount = 0.3f; // Target light intensity when dimmed (0 = dark, 1 = bright)
    
    private GameObject finalBossInstance;
    private bool bossActivated = false;
    private bool bossFullyActive = false; // True only when boss AI is enabled
    private Vector3 bossFinalPosition;
    private Light mainLight;
    private float originalLightIntensity;
    private Color originalLightColor;
    private Color originalAmbientLight;
    private Color originalFogColor;
    private float originalSkyboxExposure;

    [Header("Wave System")]
    [SerializeField]
    private int maxTotalAliens = 15; // Maximum aliens that will spawn
    
    [SerializeField]
    private float initialWaveDelay = 8f; // Delay before first wave
    
    [SerializeField]
    private float timeBetweenWaves = 10f; // Time between waves at start
    
    [SerializeField]
    private float minTimeBetweenWaves = 3f; // Minimum time between waves (when difficulty maxed)
    
    [SerializeField]
    private float delayReductionRate = 0.5f; // How much to reduce delay per wave
    
    [SerializeField]
    private int initialEnemiesPerWave = 1; // Starting enemies per wave
    
    [SerializeField]
    private int maxEnemiesPerWave = 8; // Maximum enemies per wave
    
    [SerializeField]
    private float enemyIncreaseRate = 0.5f; // How much to increase enemies per wave
    
    [SerializeField]
    private float timeBetweenSpawnsInWave = 1f; // Delay between individual enemies in a wave
    
    [Header("Spawn Position")]
    [SerializeField]
    private float spawnDistance = 8f; // Reduced from 15f to spawn closer to player

    [SerializeField]
    private float spawnHeightMin = 10f;

    [SerializeField]
    private float spawnHeightMax = 14f;

    [SerializeField]
    private float hoverBobSpeed = 1f;

    [SerializeField]
    private float hoverBobAmount = 0.3f;

    [SerializeField]
    private float hoverDurationMin = 3f;

    [SerializeField]
    private float hoverDurationMax = 6f;

    [SerializeField]
    private float attackSpeed = 5f;

    [SerializeField]
    private float hoverAboveDistance = 2f;

    [SerializeField]
    private float abductionSpeed = 3f;

    [SerializeField]
    private float abductionDuration = 3f;
    
    [Header("Beam Settings")]
    [SerializeField]
    private float beamOpenDelay = 0.5f; // Delay before beam starts opening animation

    [Header("Spawn Effects")]
    [SerializeField]
    private GameObject spawnEffectPrefab; // Particle effect when UFO spawns
    
    [SerializeField]
    private float spawnEffectDuration = 2f; // How long the spawn effect lasts

    [Header("UFO Sounds")]
    [SerializeField]
    private AudioClip ufoPopSound;
    
    [SerializeField]
    private AudioClip ufoMoveSound;
    
    [SerializeField]
    private AudioClip sheepScreamSound;
    
    [SerializeField]
    private AudioClip bossRumbleSound;
    
    [SerializeField]
    private AudioClip bossMusicSound;

    private Transform playerCamera;
    private List<GameObject> moutonInstances = new List<GameObject>();
    
    // Wave tracking variables
    private int currentWaveNumber = 0;
    private float currentWaveDelay;
    private float currentEnemiesPerWave;
    private int totalAliensSpawned = 0;
    private bool allWavesCompleted = false;

    private bool isBowGrabbed = false;
    private bool isQuiverEquipped = false;
    private bool hasGameStarted = false;

    [Header("Countdown")]
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private AudioClip beepSound;
    [SerializeField] private AudioClip goSound;

    void Start()
    {
        playerCamera = Camera.main.transform;
        
        // Fix camera rotation - ensure XR Rig/Camera Offset is facing forward
        if (playerCamera != null && playerCamera.parent != null)
        {
            Transform cameraOffset = playerCamera.parent;
            // Reset the Camera Offset rotation to face forward (0, 0, 0)
            cameraOffset.localRotation = Quaternion.identity;
            // Debug.Log($"Reset {cameraOffset.name} rotation to face forward");
            
            // Also reset the XR Origin/Rig (parent of Camera Offset) if it exists
            if (cameraOffset.parent != null)
            {
                Transform xrOrigin = cameraOffset.parent;
                xrOrigin.localRotation = Quaternion.identity;
                // Debug.Log($"Reset {xrOrigin.name} rotation to face forward");
            }
        }
        
        FindAllMoutons();
        
        // Configure camera for outdoor skybox rendering
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.clearFlags = CameraClearFlags.Skybox;
            // Debug.Log("Camera configured for skybox rendering");
        }
        
        // Find or create the main directional light (sun)
        Light[] allLights = FindObjectsOfType<Light>();
        foreach (Light light in allLights)
        {
            if (light.type == LightType.Directional)
            {
                mainLight = light;
                break;
            }
        }
        
        // If no directional light exists, create one
        if (mainLight == null)
        {
            GameObject sunObject = new GameObject("Directional Light (Sun)");
            mainLight = sunObject.AddComponent<Light>();
            mainLight.type = LightType.Directional;
            
            // Position the sun (rotation determines light direction)
            sunObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            
            // Debug.Log("Created new Directional Light for sunny day");
        }
        
        // Configure the light for pleasant sunny day
        mainLight.color = new Color(1f, 0.96f, 0.84f); // Warm sunny color
        mainLight.intensity = 1f; // Pleasant sunny brightness
        mainLight.shadows = LightShadows.Soft;
        mainLight.shadowStrength = 0.3f; // Softer, less harsh shadows
        
        // Store original values
        originalLightIntensity = mainLight.intensity;
        originalLightColor = mainLight.color;
        // Debug.Log($"Sun configured with intensity: {originalLightIntensity}, color: {originalLightColor}");
        
        // Set up bright sunny environment lighting
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 1.5f; // Much higher ambient to minimize harsh shading
        RenderSettings.reflectionIntensity = 0.5f; // Add reflections for better lighting
        
        // Set up skybox with pleasant exposure
        if (RenderSettings.skybox != null)
        {
            RenderSettings.skybox.SetFloat("_Exposure", 1.1f); // Pleasant sky brightness
            RenderSettings.sun = mainLight; // Set the sun source
            originalSkyboxExposure = 1.1f;
            // Debug.Log($"Skybox configured with exposure: {originalSkyboxExposure}");
        }
        else
        {
            originalSkyboxExposure = 1.1f;
            // Debug.LogWarning("No skybox material found! Go to Window > Rendering > Lighting > Environment and assign a Skybox Material");
        }
        
        // Store original ambient light and fog color
        originalAmbientLight = RenderSettings.ambientLight;
        originalFogColor = RenderSettings.fogColor;
        
        // Debug.Log($"Sunny day setup complete! Ambient mode: {RenderSettings.ambientMode}");
        
        // Initialize wave system
        currentWaveDelay = timeBetweenWaves;
        currentEnemiesPerWave = initialEnemiesPerWave;
        
        // Spawn and hide the final boss
        SpawnFinalBoss();
        
        // StartCoroutine(SpawnWaveRoutine());
    }

    // Called by the bow when he is caught
    public void PlayerGrabbedBow()
    {
        isBowGrabbed = true;
        TryStartInvasion();
    }

    // Called by the quiver when it is equipped
    public void PlayerEquippedQuiver()
    {
        isQuiverEquipped = true;
        TryStartInvasion();
    }

    // Check if we have the two green lights
    private void TryStartInvasion()
    {
        if (isBowGrabbed && isQuiverEquipped && !hasGameStarted)
        {
            hasGameStarted = true;
            
            StartCoroutine(StartGameCountdown());
        }
    }

    private IEnumerator StartGameCountdown()
    {
        if (countdownText != null)
        {
            // Make sure that the text (or its parent Canvas) is lit
            countdownText.gameObject.SetActive(true);
            
            // Loop for 3, 2, 1
            for (int i = 3; i > 0; i--)
            {
                countdownText.text = i.ToString();
                
                // Play sound
                if (beepSound != null && playerCamera != null)
                {
                    AudioSource.PlayClipAtPoint(beepSound, playerCamera.position, 1f);
                }
                
                // Wait one second
                yield return new WaitForSeconds(1f);
            }
            
            // Display "GO !"
            countdownText.text = "GO !";
            if (goSound != null && playerCamera != null)
            {
                AudioSource.PlayClipAtPoint(goSound, playerCamera.position, 1f);
            }
            
            // Leave the "GO !" visible for 1 second
            yield return new WaitForSeconds(1f);
            
            // Hide text
            countdownText.gameObject.SetActive(false);
        }
        else
        {
            // Security: if we haven’t put any text, we just wait 3 seconds in silence
            yield return new WaitForSeconds(3f);
        }

        StartCoroutine(SpawnWaveRoutine());
    }

    void Update()
    {
        // Check if all waves are done and all regular aliens are defeated
        if (allWavesCompleted && !bossActivated && finalBossInstance != null)
        {
            // Count only regular aliens (FlyingSaucerHover components that were added by SpawnEnemy)
            int regularAlienCount = FindObjectsOfType<FlyingSaucerHover>().Length;
            
            if (regularAlienCount == 0)
            {
                // Debug.Log("All regular aliens defeated! Final boss rising...");
                StartCoroutine(ActivateFinalBoss());
                bossActivated = true;
            }
        }
    }

    void FindAllMoutons()
    {
        // Find all mouton instances in the scene based on the prefab
        moutonInstances.Clear();
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            // Check if this object's name matches the mouton prefab name
            if (obj.name.Contains(moutonPrefab.name))
            {
                moutonInstances.Add(obj);
            }
        }
    }

    private IEnumerator SpawnWaveRoutine()
    {
        // Wait before the first wave
        yield return new WaitForSeconds(initialWaveDelay);
        
        while (totalAliensSpawned < maxTotalAliens)
        {
            currentWaveNumber++;
            
            // Calculate how many enemies to spawn in this wave
            int enemiesToSpawn = Mathf.RoundToInt(currentEnemiesPerWave);
            enemiesToSpawn = Mathf.Clamp(enemiesToSpawn, 1, maxEnemiesPerWave);
            
            // Don't spawn more than the limit
            int remainingAliens = maxTotalAliens - totalAliensSpawned;
            enemiesToSpawn = Mathf.Min(enemiesToSpawn, remainingAliens);
            
            // Debug.Log($"Wave {currentWaveNumber}: Spawning {enemiesToSpawn} enemies ({totalAliensSpawned + enemiesToSpawn}/{maxTotalAliens} total)");
            
            // Spawn all enemies in this wave with small delays between them
            for (int i = 0; i < enemiesToSpawn; i++)
            {
                SpawnEnemy();
                totalAliensSpawned++;
                
                // Wait before spawning next enemy in wave (unless it's the last one)
                if (i < enemiesToSpawn - 1)
                {
                    yield return new WaitForSeconds(timeBetweenSpawnsInWave);
                }
            }
            
            // Check if we've reached the limit
            if (totalAliensSpawned >= maxTotalAliens)
            {
                // Debug.Log($"All {maxTotalAliens} aliens have been spawned! Final boss will rise after they're defeated...");
                allWavesCompleted = true;
                break;
            }
            
            // Increase difficulty for next wave
            currentEnemiesPerWave += enemyIncreaseRate;
            currentWaveDelay = Mathf.Max(minTimeBetweenWaves, currentWaveDelay - delayReductionRate);
            
            // Wait before next wave
            yield return new WaitForSeconds(currentWaveDelay);
        }
    }

    private void SpawnEnemy()
    {
        if (playerCamera == null) return;

        // Check if we have any OVNI prefabs
        if (ovniPrefabs == null || ovniPrefabs.Length == 0)
        {
            // Debug.LogError("No OVNI prefabs assigned to EnemySpawner!");
            return;
        }

        // Randomly select one of the 3 OVNI types
        int randomOvniIndex = Random.Range(0, ovniPrefabs.Length);
        GameObject selectedOvniPrefab = ovniPrefabs[randomOvniIndex];

        // Calculate spawn position in front of the player (within a narrower arc)
        // Get the player's forward direction on the horizontal plane
        Vector3 cameraForward = playerCamera.forward;
        cameraForward.y = 0f; // Flatten to horizontal plane
        cameraForward.Normalize();
        
        // Random angle offset from -60 to +60 degrees (narrower front arc to avoid trees)
        float angleOffset = Random.Range(-60f, 60f);
        
        // Rotate the forward direction by the offset
        float angleInRadians = angleOffset * Mathf.Deg2Rad;
        Vector3 randomDirection = new Vector3(
            cameraForward.x * Mathf.Cos(angleInRadians) - cameraForward.z * Mathf.Sin(angleInRadians),
            0f,
            cameraForward.x * Mathf.Sin(angleInRadians) + cameraForward.z * Mathf.Cos(angleInRadians)
        );
        
        Vector3 spawnPosition = playerCamera.position + randomDirection * spawnDistance;
        spawnPosition.y = playerCamera.position.y + Random.Range(spawnHeightMin, spawnHeightMax);

        // Instantiate the randomly selected enemy
        GameObject newEnemy = Instantiate(selectedOvniPrefab, spawnPosition, Quaternion.identity);

        // Spawn particle effect at spawn location
        if (spawnEffectPrefab != null)
        {
            GameObject spawnEffect = Instantiate(spawnEffectPrefab, spawnPosition, Quaternion.identity);
            Destroy(spawnEffect, spawnEffectDuration);
        }

        // Add flying saucer hover behavior to the enemy
        FlyingSaucerHover hoverScript = newEnemy.GetComponent<FlyingSaucerHover>();
        if (hoverScript == null)
        {
            hoverScript = newEnemy.AddComponent<FlyingSaucerHover>();
        }
        
        float hoverDuration = Random.Range(hoverDurationMin, hoverDurationMax);
        hoverScript.Initialize(hoverBobSpeed, hoverBobAmount, hoverDuration, attackSpeed, hoverAboveDistance, abductionSpeed, abductionDuration, moutonInstances, ufoPopSound, ufoMoveSound, sheepScreamSound, beamOpenDelay);
    }

    public void RefreshMoutonList()
    {
        FindAllMoutons();
    }
    
    public bool IsSpawningStopped()
    {
        return totalAliensSpawned >= maxTotalAliens;
    }
    
    private void SpawnFinalBoss()
    {
        // Find the existing OVNI_3_final game object in the scene
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("OVNI_3_final"))
            {
                finalBossInstance = obj;
                break;
            }
        }
        
        if (finalBossInstance == null)
        {
            // Debug.LogError("OVNI_3_final not found in scene!");
            return;
        }
        
        // Disable the boss components initially
        FlyingSaucerHover bossHover = finalBossInstance.GetComponent<FlyingSaucerHover>();
        if (bossHover != null)
        {
            bossHover.enabled = false;
        }

        // Disable the Hitbox (the arrow will pass through)
        Collider col = finalBossInstance.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Disable the visual (makes it invisible)
        Renderer[] renderers = finalBossInstance.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = false;
        }
        
        // Disable particle effects (like the tractor beam)
        ParticleSystem[] particles = finalBossInstance.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem p in particles)
        {
            p.Stop();
        }
        
        // Debug.Log("Final boss found in scene and ready to activate");
    }
    
    private IEnumerator ActivateFinalBoss()
    {
        if (finalBossInstance == null) yield break;
        
        // Reactivate the Hitbox
        Collider col = finalBossInstance.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        // RReactivate the visual
        Renderer[] renderers = finalBossInstance.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = true;
        }
        // Debug.Log("Final boss rising!");
        
        // Play boss music from the beginning
        if (bossMusicSound != null)
        {
            AudioSource.PlayClipAtPoint(bossMusicSound, playerCamera.position, 0.5f);
        }
        
        // Start camera shake and dim the light
        StartCoroutine(ShakeCamera(cameraShakeDuration));
        StartCoroutine(DimLight(cameraShakeDuration));
        
        // Phase 1: Rise up vertically
        Vector3 startPosition = finalBossInstance.transform.position;
        float targetRiseHeight = startPosition.y + bossRiseHeight;
        
        while (finalBossInstance.transform.position.y < targetRiseHeight)
        {
            Vector3 pos = finalBossInstance.transform.position;
            pos.y += bossRiseSpeed * Time.deltaTime;
            finalBossInstance.transform.position = pos;
            yield return null;
        }
        
        // Debug.Log("Final boss reached peak - waiting before attack...");
        
        // Wait at the top for a few seconds (menacing pause)
        yield return new WaitForSeconds(3f);
        
        // Debug.Log("Final boss moving in front of player!");
        
        // Phase 2: Move to a position in front of the player, keeping current height
        Vector3 playerPosition = Camera.main.transform.position;
        Vector3 playerForward = Camera.main.transform.forward;
        playerForward.y = 0; // Keep horizontal
        playerForward.Normalize();
        
        Vector3 targetPosition = playerPosition + playerForward * bossDistanceFromPlayer;
        targetPosition.y = finalBossInstance.transform.position.y; // Stay at current height
        
        ParticleSystem bossBeam = finalBossInstance.GetComponentInChildren<ParticleSystem>();
        
        // Hide beam initially
        if (bossBeam != null)
        {
            bossBeam.Stop();
            var emission = bossBeam.emission;
            emission.enabled = false;
        }
        
        while (Vector3.Distance(finalBossInstance.transform.position, targetPosition) > 2f)
        {
            finalBossInstance.transform.position = Vector3.MoveTowards(
                finalBossInstance.transform.position,
                targetPosition,
                bossRiseSpeed * Time.deltaTime
            );
            
            yield return null;
        }
        
        // Particle beam disabled for final boss
        // if (bossBeam != null)
        // {
        //     bossBeam.Play();
        // }
        
        // Mark boss as fully active
        bossFullyActive = true;
        
        // Debug.Log("Final boss starting to abduct ALL remaining sheep!");
        
        // Start abducting all sheep at once
        StartCoroutine(BossAbductAllSheep());
    }
    
    private Vector3 CalculateSheepCenter()
    {
        MoutonMovement[] allSheep = FindObjectsOfType<MoutonMovement>();
        
        if (allSheep.Length == 0)
        {
            // If no sheep, return a default position
            return new Vector3(0, 0, 0);
        }
        
        Vector3 center = Vector3.zero;
        foreach (MoutonMovement sheep in allSheep)
        {
            center += sheep.transform.position;
        }
        center /= allSheep.Length;
        
        return center;
    }
    
    private IEnumerator BossAbductAllSheep()
    {
        // Wait before starting abduction
        yield return new WaitForSeconds(bossAbductionDelay);
        
        // Get all remaining sheep
        MoutonMovement[] allSheep = FindObjectsOfType<MoutonMovement>();
        List<GameObject> sheepObjects = new List<GameObject>();
        
        foreach (MoutonMovement sheep in allSheep)
        {
            sheepObjects.Add(sheep.gameObject);
            
            // Disable physics on each sheep
            Rigidbody rb = sheep.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }
        }
        
        // Debug.Log($"Boss is lifting {sheepObjects.Count} sheep!");
        
        float rotationSpeed = 60f; // Boss rotation speed
        float abductionTimer = 0f; // Track abduction time
        
        // Calculate speed needed to abduct all sheep within duration
        float effectiveSpeed = sheepRiseSpeed;
        
        // Lift all sheep toward the boss
        while (finalBossInstance != null && sheepObjects.Count > 0 && abductionTimer < bossAbductionDuration)
        {
            abductionTimer += Time.deltaTime;
            
            // Rotate the boss continuously
            finalBossInstance.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            
            Vector3 bossPosition = finalBossInstance.transform.position;
            
            // Move all sheep upward and toward boss center
            for (int i = sheepObjects.Count - 1; i >= 0; i--)
            {
                if (sheepObjects[i] == null)
                {
                    sheepObjects.RemoveAt(i);
                    continue;
                }
                
                Vector3 sheepPos = sheepObjects[i].transform.position;
                
                // Calculate direction toward boss (like being pulled by a cone beam)
                Vector3 directionToBoss = (bossPosition - sheepPos).normalized;
                
                // Move sheep toward boss center and upward
                sheepPos += directionToBoss * effectiveSpeed * Time.deltaTime;
                sheepObjects[i].transform.position = sheepPos;
                
                // If sheep reached the boss, destroy it
                if (Vector3.Distance(sheepPos, bossPosition) < 2f)
                {
                    Destroy(sheepObjects[i]);
                    sheepObjects.RemoveAt(i);
                    // Debug.Log($"Sheep abducted! {sheepObjects.Count} remaining");
                }
            }
            
            yield return null;
        }
        
        // Force destroy any remaining sheep if duration ran out
        foreach (GameObject sheep in sheepObjects)
        {
            if (sheep != null)
            {
                Destroy(sheep);
            }
        }
        
        // Debug.Log("Boss has abducted all sheep!");
    }
    
    public void StopBossEffects()
    {
        // Stop all boss-related coroutines
        StopAllCoroutines();
        
        // Reset camera shake if active
        if (playerCamera != null && playerCamera.parent != null)
        {
            Transform rigTransform = playerCamera.parent;
            rigTransform.localPosition = Vector3.zero; // Reset to original position
        }
        
        // Destroy boss if it exists
        if (finalBossInstance != null)
        {
            Destroy(finalBossInstance);
        }
    }
    
    public bool IsBossActive()
    {
        return bossFullyActive;
    }
    
    public bool DoesBossExist()
    {
        if (finalBossInstance == null) return false;

        // Si le boss est physiquement là, mais que son script dit qu'il est mort, 
        // on renvoie "false" pour déclencher l'écran de victoire !
        BossTarget bossTarget = finalBossInstance.GetComponent<BossTarget>();
        if (bossTarget != null && bossTarget.isCrashFinished) return false;
        
        return true;
    }
    
    private IEnumerator ShakeCamera(float duration)
    {
        // Play rumble sound when shaking starts
        if (bossRumbleSound != null)
        {
            AudioSource.PlayClipAtPoint(bossRumbleSound, playerCamera.position, 0.3f);
        }
        
        // In VR, shake the parent rig instead of the camera (which is controlled by headset)
        Transform rigTransform = playerCamera.parent;
        
        if (rigTransform == null)
        {
            // Debug.LogWarning("Camera has no parent - cannot shake in VR mode");
            yield break;
        }
        
        Vector3 originalPosition = rigTransform.localPosition;
        Vector3 originalBowPosition = Vector3.zero;
        bool hasBow = false;
        
        // Store bow's original position if available
        if (bowObject != null)
        {
            originalBowPosition = bowObject.transform.localPosition;
            hasBow = true;
        }
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            // Shake the XR Rig position - this moves the entire play space
            float x = Random.Range(-1f, 1f) * cameraShakeIntensity;
            float y = Random.Range(-1f, 1f) * cameraShakeIntensity;
            float z = Random.Range(-1f, 1f) * cameraShakeIntensity;
            
            Vector3 shakeOffset = new Vector3(x, y, z);
            rigTransform.localPosition = originalPosition + shakeOffset;
            
            // Counter-shake the bow to keep it stable
            if (hasBow)
            {
                bowObject.transform.localPosition = originalBowPosition - shakeOffset;
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        rigTransform.localPosition = originalPosition;
        
        // Restore bow position
        if (hasBow)
        {
            bowObject.transform.localPosition = originalBowPosition;
        }
    }
    
    private IEnumerator DimLight(float duration)
    {
        float elapsed = 0f;
        
        // Target values for menacing/dark atmosphere
        Color darkerSunColor = originalLightColor * 0.5f; // Dim the sun color significantly
        Color darkerAmbient = originalAmbientLight * 0.3f; // Very dark ambient
        float targetSkyboxExposure = originalSkyboxExposure * 0.25f; // Dim skybox a lot
        float targetIntensity = originalLightIntensity * lightDimAmount;
        
        // Debug.Log($"Dimming from sunny day - Light: {originalLightIntensity} to {targetIntensity}, Ambient: {originalAmbientLight} to {darkerAmbient}");
        
        // Dim everything over the duration
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            
            // Dim directional light (sun)
            if (mainLight != null)
            {
                mainLight.intensity = Mathf.Lerp(originalLightIntensity, targetIntensity, t);
                mainLight.color = Color.Lerp(originalLightColor, darkerSunColor, t);
            }
            
            // Dim ambient light dramatically
            RenderSettings.ambientLight = Color.Lerp(originalAmbientLight, darkerAmbient, t);
            
            // Darken fog if enabled
            if (RenderSettings.fog)
            {
                Color darkerFog = originalFogColor * 0.4f;
                RenderSettings.fogColor = Color.Lerp(originalFogColor, darkerFog, t);
            }
            
            // Dim the skybox (darkens sun and sky)
            if (RenderSettings.skybox != null)
            {
                float newExposure = Mathf.Lerp(originalSkyboxExposure, targetSkyboxExposure, t);
                RenderSettings.skybox.SetFloat("_Exposure", newExposure);
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Set final dark/menacing values
        if (mainLight != null)
        {
            mainLight.intensity = targetIntensity;
            mainLight.color = darkerSunColor;
        }
        RenderSettings.ambientLight = darkerAmbient;
        if (RenderSettings.fog)
        {
            RenderSettings.fogColor = originalFogColor * 0.4f;
        }
        if (RenderSettings.skybox != null)
        {
            RenderSettings.skybox.SetFloat("_Exposure", targetSkyboxExposure);
        }
        
        // Debug.Log("Scene darkened - menacing atmosphere active!");
    }
}

// Component that makes enemies hover in place like flying saucers, then attack moutons
public class FlyingSaucerHover : MonoBehaviour
{
    private Vector3 startPosition;
    private Quaternion initialRotation;
    private float bobSpeed;
    private float bobAmount;
    private float randomOffset;
    private float hoverDuration;
    private float attackSpeed;
    private float spawnTime;
    private float hoverAboveDistance;
    private float abductionSpeed;
    private float abductionDuration;
    
    private bool isAttacking = false;
    private bool isAbducting = false;
    private bool hasPickedUp = false;
    private float abductionStartTime;
    private Vector3 hoverTargetPosition;
    private Transform targetMouton;
    private GameObject capturedMouton;
    private float moutonGroundY;
    private List<GameObject> moutonList;
    private ParticleSystem abductionBeam;
    private GameObject beamObject;
    private GameObject beamWrapper; // Wrapper for centered scaling
    private Renderer[] beamRenderers;
    private AudioSource audioSource;
    private float timeAboveSheep = -1f;
    private float beamDelay = 10f;
    private float beamOpenDelay = 0.5f; // Local copy for each alien
    private bool isBeamAnimating = false;
    private float beamAnimationProgress = 0f;
    private Vector3 beamOriginalScale;
    private Vector3 beamOriginalPosition;
    
    [SerializeField]
    private AudioClip popSound;
    
    [SerializeField]
    private AudioClip moveSound;
    
    [SerializeField]
    private AudioClip sheepScreamSound;

    public void Initialize(float bobSpd, float bobAmt, float hoverDur, float atkSpd, float hoverDist, float abductSpd, float abductDur, List<GameObject> moutons, AudioClip popSnd, AudioClip moveSnd, AudioClip sheepScream, float beamDelay)
    {
        bobSpeed = bobSpd;
        bobAmount = bobAmt;
        hoverDuration = hoverDur;
        attackSpeed = atkSpd;
        hoverAboveDistance = hoverDist;
        abductionSpeed = abductSpd;
        abductionDuration = abductDur;
        moutonList = moutons;
        popSound = popSnd;
        moveSound = moveSnd;
        sheepScreamSound = sheepScream;
        beamOpenDelay = beamDelay;
        randomOffset = Random.Range(0f, 100f);
        spawnTime = Time.time;
    }

    void Start()
    {
        startPosition = transform.position;
        initialRotation = transform.rotation;
        
        // Pop-in animation: start small and grow
        StartCoroutine(PopInAnimation());
        
        // Find the "Beam" object by searching through all children recursively
        Transform beamTransform = FindChildRecursive(transform, "Beam");
        if (beamTransform != null)
        {
            beamObject = beamTransform.gameObject;
            abductionBeam = beamTransform.GetComponent<ParticleSystem>();
            beamRenderers = beamTransform.GetComponentsInChildren<Renderer>(true);
            
            // Create a wrapper GameObject at the center of the beam's bounds for proper scaling
            beamWrapper = new GameObject("BeamWrapper");
            beamWrapper.transform.SetParent(transform);
            
            // Get the world center of the beam
            Renderer beamRenderer = beamObject.GetComponent<Renderer>();
            if (beamRenderer != null)
            {
                Vector3 worldCenter = beamRenderer.bounds.center;
                beamWrapper.transform.position = worldCenter;
            }
            else
            {
                beamWrapper.transform.position = beamObject.transform.position;
            }
            
            beamWrapper.transform.rotation = beamObject.transform.rotation;
            
            // Store original states
            beamOriginalScale = beamWrapper.transform.localScale;
            beamOriginalPosition = beamWrapper.transform.localPosition;
            
            // Reparent beam to wrapper
            Vector3 beamWorldPos = beamObject.transform.position;
            Quaternion beamWorldRot = beamObject.transform.rotation;
            Vector3 beamWorldScale = beamObject.transform.lossyScale;
            
            beamObject.transform.SetParent(beamWrapper.transform);
            
            // Maintain visual position
            beamObject.transform.position = beamWorldPos;
            beamObject.transform.rotation = beamWorldRot;
            
            // Debug.Log($"Beam wrapper created at position: {beamOriginalPosition}, scale: {beamOriginalScale}");
            
            // Completely hide the beam initially
            HideBeam();
            
            // Debug.Log($"Beam found in {gameObject.name} with {beamRenderers.Length} renderers");
        }
        else
        {
            // Debug.LogWarning($"Beam object not found in {gameObject.name} hierarchy!");
        }
        
        // Setup audio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f; // 3D sound
        audioSource.minDistance = 5f;
        audioSource.maxDistance = 50f;
        
        // Play pop sound when UFO arrives
        if (popSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(popSound);
        }
    }
    
    Transform FindChildRecursive(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;
            
            Transform result = FindChildRecursive(child, childName);
            if (result != null)
                return result;
        }
        return null;
    }
    
    void ShowBeam()
    {
        if (beamWrapper == null) return;
        
        // Start beam animation only if not already visible
        if (!beamWrapper.activeSelf)
        {
            StartCoroutine(ShowBeamWithDelay());
        }
    }
    
    IEnumerator ShowBeamWithDelay()
    {
        // Activate wrapper but keep invisible
        beamWrapper.SetActive(true);
        beamWrapper.transform.localScale = new Vector3(0.01f, beamOriginalScale.y, 0.01f);
        
        // Make renderers visible but beam is still tiny
        if (beamRenderers != null)
        {
            foreach (Renderer renderer in beamRenderers)
            {
                renderer.enabled = true;
            }
        }
        
        if (abductionBeam != null)
        {
            var emission = abductionBeam.emission;
            emission.enabled = true;
            if (!abductionBeam.isPlaying)
            {
                abductionBeam.Play();
            }
        }
        
        // Wait before starting the opening animation
        yield return new WaitForSeconds(beamOpenDelay);
        
        // Now start the opening animation
        isBeamAnimating = true;
        beamAnimationProgress = 0f;
    }
    
    void HideBeam()
    {
        if (beamWrapper == null) return;
        
        // Reset animation state
        isBeamAnimating = false;
        beamAnimationProgress = 0f;
        
        // Reset scale - XZ to tiny, Y stays original
        if (beamWrapper.transform != null)
        {
            beamWrapper.transform.localScale = new Vector3(0.01f, beamOriginalScale.y, 0.01f);
        }
        
        if (abductionBeam != null)
        {
            abductionBeam.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var emission = abductionBeam.emission;
            emission.enabled = false;
        }
        
        if (beamRenderers != null)
        {
            foreach (Renderer renderer in beamRenderers)
            {
                renderer.enabled = false;
            }
        }
        
        beamWrapper.SetActive(false);
    }

    void Update()
    {
        // Slowly rotate the UFO around its local Y axis
        transform.Rotate(0, 20f * Time.deltaTime, 0, Space.Self);
        // Update the initial rotation so it doesn't get reset
        initialRotation = transform.rotation;
        
        // Animate beam opening up (expand width and length, keep height)
        if (isBeamAnimating && beamWrapper != null && beamWrapper.activeSelf)
        {
            beamAnimationProgress += Time.deltaTime * 5f; // Speed of beam expansion
            
            if (beamAnimationProgress >= 1f)
            {
                // Animation complete
                beamAnimationProgress = 1f;
                isBeamAnimating = false;
                beamWrapper.transform.localScale = beamOriginalScale;
            }
            else
            {
                // Scale only X and Z from 0 to full, keep Y at original
                float currentXZScale = Mathf.Lerp(0.01f, 1f, beamAnimationProgress);
                beamWrapper.transform.localScale = new Vector3(
                    beamOriginalScale.x * currentXZScale,
                    beamOriginalScale.y,
                    beamOriginalScale.z * currentXZScale
                );
            }
        }
        
        // Check if hover duration has passed
        if (!isAttacking && Time.time - spawnTime >= hoverDuration)
        {
            StartAttacking();
        }

        if (isAbducting)
        {
            // Abduction phase: lift sheep upward
            if (capturedMouton != null)
            {
                // Move both alien and sheep upward
                transform.position += Vector3.up * abductionSpeed * Time.deltaTime;
                
                // Lift the sheep up with the alien, maintaining horizontal alignment
                Vector3 sheepPos = capturedMouton.transform.position;
                sheepPos.x = transform.position.x;
                sheepPos.z = transform.position.z;
                sheepPos.y += abductionSpeed * Time.deltaTime; // Lift from ground
                capturedMouton.transform.position = sheepPos;
                
                // Keep the original rotation (stay level)
                transform.rotation = initialRotation;
                
                // Keep beam active during abduction (always above the sheep)
                ShowBeam();

                // Check if abduction duration is over
                if (Time.time - abductionStartTime >= abductionDuration)
                {
                    // Destroy both the alien and the sheep
                    Destroy(capturedMouton);
                    Destroy(gameObject);
                }
            }
            else
            {
                // Sheep was destroyed somehow, just destroy the alien
                Destroy(gameObject);
            }
        }
        else if (isAttacking)
        {
            if (targetMouton != null)
            {
                if (!hasPickedUp)
                {
                    // Phase 1: Fly to position above the mouton
                    hoverTargetPosition = targetMouton.position + Vector3.up * hoverAboveDistance;
                    Vector3 direction = (hoverTargetPosition - transform.position).normalized;
                    transform.position += direction * attackSpeed * Time.deltaTime;
                    
                    // Keep the original rotation (stay level)
                    transform.rotation = initialRotation;

                    // Check if close enough to pick up
                    float distanceToTarget = Vector3.Distance(transform.position, hoverTargetPosition);
                    
                    // Check horizontal distance to sheep (only show beam when above the sheep)
                    Vector3 horizontalPos = new Vector3(transform.position.x, 0, transform.position.z);
                    Vector3 sheepHorizontalPos = new Vector3(targetMouton.position.x, 0, targetMouton.position.z);
                    float horizontalDistance = Vector3.Distance(horizontalPos, sheepHorizontalPos);
                    
                    // Only show beam when directly above the sheep (within 2 units horizontally)
                    if (horizontalDistance < 2f)
                    {
                        // Start timer if just arrived above sheep
                        if (timeAboveSheep < 0)
                        {
                            timeAboveSheep = Time.time;
                        }
                        
                        // Show beam only after 2 second delay
                        if (Time.time - timeAboveSheep >= beamDelay)
                        {
                            ShowBeam();
                        }
                    }
                    else
                    {
                        // Reset timer when not above sheep
                        timeAboveSheep = -1f;
                        HideBeam();
                    }
                    
                    if (distanceToTarget < 0.5f)
                    {
                        PickUpMouton();
                    }
                }
                else
                {
                    // Phase 2: Hover in place with the sheep
                    // Keep sheep on the ground below the alien
                    if (capturedMouton != null)
                    {
                        Vector3 sheepPos = capturedMouton.transform.position;
                        sheepPos.x = transform.position.x;
                        sheepPos.z = transform.position.z;
                        sheepPos.y = moutonGroundY; // Keep at ground level
                        capturedMouton.transform.position = sheepPos;
                        
                        // Beam should always be active when we have a captured sheep (always above it)
                        ShowBeam();
                    }

                    // Gentle bobbing while carrying
                    float bobOffset = Mathf.Sin((Time.time + randomOffset) * bobSpeed * 0.5f) * bobAmount * 0.5f;
                    transform.position = hoverTargetPosition + Vector3.up * bobOffset;
                    
                    // Keep the original rotation (stay level)
                    transform.rotation = initialRotation;
                }
            }
        }
        else
        {
            // Initial hover phase
            // Gentle bobbing motion up and down
            float bobOffset = Mathf.Sin((Time.time + randomOffset) * bobSpeed) * bobAmount;
            transform.position = startPosition + Vector3.up * bobOffset;
            
            // Keep the original rotation (stay level)
            transform.rotation = initialRotation;
            
            // Ensure beam is off during initial hover (not above any sheep yet)
            HideBeam();
        }
    }

    void PickUpMouton()
    {
        hasPickedUp = true;
        capturedMouton = targetMouton.gameObject;
        moutonGroundY = capturedMouton.transform.position.y; // Store the ground level
        
        // Disable physics on the mouton if it has a Rigidbody
        Rigidbody rb = capturedMouton.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // Start abduction after a short delay
        StartCoroutine(StartAbductionAfterDelay(2f));
    }

    void OnDestroy()
    {
        // Stop the beam effect
        HideBeam();
        
        // Release the mouton if this alien is destroyed before capturing
        if (targetMouton != null && !hasPickedUp)
        {
            MoutonMovement moutonScript = targetMouton.GetComponent<MoutonMovement>();
            if (moutonScript != null)
            {
                moutonScript.SetTargeted(false);
            }
        }
        
        // Release captured sheep when UFO is destroyed
        if (capturedMouton != null)
        {
            // Unparent the sheep in case it was parented to the UFO
            capturedMouton.transform.SetParent(null);
            
            // Prepare for manual falling (no physics)
            Rigidbody rb = capturedMouton.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Keep kinematic off, but we'll use manual movement for falling
                rb.isKinematic = false;
                rb.useGravity = false; // Disable gravity, we'll move manually
                rb.constraints = RigidbodyConstraints.FreezeRotation; // Allow Y movement
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
            // Release the sheep and tell it to fall manually
            MoutonMovement moutonScript = capturedMouton.GetComponent<MoutonMovement>();
            if (moutonScript != null)
            {
                moutonScript.SetTargeted(false);
                moutonScript.SetFalling(true); // This will trigger manual falling movement
            }
        }
    }

    IEnumerator PopInAnimation()
    {
        // Start at scale 0
        Vector3 targetScale = transform.localScale;
        transform.localScale = Vector3.zero;
        
        float duration = 0.3f; // Pop animation duration
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Simple ease-out back (overshoot and settle)
            float overshoot = 1.70158f;
            float scaleFactor = 1f + overshoot * Mathf.Pow(t - 1f, 3f) + Mathf.Pow(t - 1f, 2f);
            
            transform.localScale = targetScale * scaleFactor;
            yield return null;
        }
        
        // Ensure we end at exact target scale
        transform.localScale = targetScale;
    }

    IEnumerator StartAbductionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Play sheep scream when abduction starts
        if (sheepScreamSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(sheepScreamSound);
        }
        
        isAbducting = true;
        abductionStartTime = Time.time;
    }

    void StartAttacking()
    {
        isAttacking = true;
        
        // Play move sound when attacking sheep
        if (moveSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(moveSound);
        }
        
        // Remove any null references (destroyed moutons)
        moutonList.RemoveAll(item => item == null);
        
        if (moutonList.Count > 0)
        {
            // Find an untargeted mouton
            List<GameObject> availableMoutons = new List<GameObject>();
            
            foreach (GameObject mouton in moutonList)
            {
                MoutonMovement moutonScript = mouton.GetComponent<MoutonMovement>();
                if (moutonScript != null && !moutonScript.isTargeted)
                {
                    availableMoutons.Add(mouton);
                }
            }
            
            if (availableMoutons.Count > 0)
            {
                // Pick a random untargeted mouton
                int randomIndex = Random.Range(0, availableMoutons.Count);
                targetMouton = availableMoutons[randomIndex].transform;
                
                // Mark it as targeted
                MoutonMovement targetScript = targetMouton.GetComponent<MoutonMovement>();
                if (targetScript != null)
                {
                    targetScript.SetTargeted(true);
                }
            }
            else
            {
                // All moutons are already targeted, wait a bit and try again
                StartCoroutine(RetryTargeting());
            }
        }
        else
        {
            // If no moutons found, just stay hovering
            // Debug.LogWarning("No mouton instances found in scene!");
            isAttacking = false;
        }
    }
    
    IEnumerator RetryTargeting()
    {
        isAttacking = false;
        yield return new WaitForSeconds(1f);
        if (!isAbducting && !hasPickedUp)
        {
            StartAttacking();
        }
    }
}