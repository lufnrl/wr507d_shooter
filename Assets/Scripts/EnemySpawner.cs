using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject[] ovniPrefabs; // Array of 3 different OVNI types

    [SerializeField]
    private GameObject moutonPrefab;

    [SerializeField]
    private float minSpawnInterval = 2f;

    [SerializeField]
    private float maxSpawnInterval = 5f;

    [SerializeField]
    private float spawnDistance = 15f;

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

    private Transform playerCamera;
    private List<GameObject> moutonInstances = new List<GameObject>();

    void Start()
    {
        playerCamera = Camera.main.transform;
        FindAllMoutons();
        StartCoroutine(SpawnEnemyRoutine());
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

    private IEnumerator SpawnEnemyRoutine()
    {
        while (true)
        {
            // Wait for a random interval
            float randomInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(randomInterval);

            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        if (playerCamera == null) return;

        // Check if we have any OVNI prefabs
        if (ovniPrefabs == null || ovniPrefabs.Length == 0)
        {
            Debug.LogError("No OVNI prefabs assigned to EnemySpawner!");
            return;
        }

        // Randomly select one of the 3 OVNI types
        int randomOvniIndex = Random.Range(0, ovniPrefabs.Length);
        GameObject selectedOvniPrefab = ovniPrefabs[randomOvniIndex];

        // Calculate a random position in a circle around the player (horizontal plane)
        float randomAngle = Random.Range(0f, 360f);
        Vector3 randomDirection = new Vector3(
            Mathf.Cos(randomAngle * Mathf.Deg2Rad),
            0f,
            Mathf.Sin(randomAngle * Mathf.Deg2Rad)
        );
        
        Vector3 spawnPosition = playerCamera.position + randomDirection * spawnDistance;
        spawnPosition.y = playerCamera.position.y + Random.Range(spawnHeightMin, spawnHeightMax);

        // Instantiate the randomly selected enemy
        GameObject newEnemy = Instantiate(selectedOvniPrefab, spawnPosition, Quaternion.identity);

        // Add flying saucer hover behavior to the enemy
        FlyingSaucerHover hoverScript = newEnemy.GetComponent<FlyingSaucerHover>();
        if (hoverScript == null)
        {
            hoverScript = newEnemy.AddComponent<FlyingSaucerHover>();
        }
        
        float hoverDuration = Random.Range(hoverDurationMin, hoverDurationMax);
        hoverScript.Initialize(hoverBobSpeed, hoverBobAmount, hoverDuration, attackSpeed, hoverAboveDistance, abductionSpeed, abductionDuration, moutonInstances);
    }

    public void RefreshMoutonList()
    {
        FindAllMoutons();
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

    public void Initialize(float bobSpd, float bobAmt, float hoverDur, float atkSpd, float hoverDist, float abductSpd, float abductDur, List<GameObject> moutons)
    {
        bobSpeed = bobSpd;
        bobAmount = bobAmt;
        hoverDuration = hoverDur;
        attackSpeed = atkSpd;
        hoverAboveDistance = hoverDist;
        abductionSpeed = abductSpd;
        abductionDuration = abductDur;
        moutonList = moutons;
        randomOffset = Random.Range(0f, 100f);
        spawnTime = Time.time;
    }

    void Start()
    {
        startPosition = transform.position;
        initialRotation = transform.rotation;
    }

    void Update()
    {
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
                    if (Vector3.Distance(transform.position, hoverTargetPosition) < 0.5f)
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
        // Release the mouton if this alien is destroyed before capturing
        if (targetMouton != null && !hasPickedUp)
        {
            MoutonMovement moutonScript = targetMouton.GetComponent<MoutonMovement>();
            if (moutonScript != null)
            {
                moutonScript.SetTargeted(false);
            }
        }
    }

    IEnumerator StartAbductionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isAbducting = true;
        abductionStartTime = Time.time;
    }

    void StartAttacking()
    {
        isAttacking = true;
        
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
            Debug.LogWarning("No mouton instances found in scene!");
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