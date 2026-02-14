using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoutonMovement : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 1f;

    [SerializeField]
    private float rotationSpeed = 2f;

    [SerializeField]
    private float changeDirectionInterval = 3f;

    [SerializeField]
    private float wanderRadius = 10f;

    private Vector3 startPosition;
    private Vector3 targetDirection;
    private float nextDirectionChangeTime;
    private Animator animator;
    private Rigidbody rb;
    private bool isWalking = false;

    // Track if this mouton is being targeted by an alien
    public bool isTargeted { get; private set; }

    public bool isGameFinished { get; private set; } = false;

    void Start()
    {
        startPosition = transform.position;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        
        // Configure rigidbody for proper collision
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            rb.drag = 5f;
        }
        
        ChooseNewDirection();
    }

    void Update()
    {
        if (isGameFinished) return;

        // Don't move if being targeted/captured
        if (isTargeted)
        {
            // Stop walking animation when captured
            if (animator != null && isWalking)
            {
                // Try to stop the animation by playing an idle state or resetting
                AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
                animator.speed = 0; // Pause the animator
                isWalking = false;
            }
            return;
        }

        // Resume animator speed if it was paused
        if (animator != null && animator.speed == 0)
        {
            animator.speed = 1;
        }

        // Move in the current direction using Rigidbody for proper physics
        if (rb != null)
        {
            Vector3 newPosition = rb.position + targetDirection * moveSpeed * Time.deltaTime;
            rb.MovePosition(newPosition);
        }
        else
        {
            // Fallback if no Rigidbody
            transform.position += targetDirection * moveSpeed * Time.deltaTime;
        }

        // Play walking animation while moving
        if (animator != null && !isWalking)
        {
            animator.Play("walk_forward");
            isWalking = true;
        }

        // Rotate to face movement direction
        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Check if too far from start position
        if (Vector3.Distance(transform.position, startPosition) > wanderRadius)
        {
            // Head back toward start
            targetDirection = (startPosition - transform.position).normalized;
            nextDirectionChangeTime = Time.time + changeDirectionInterval;
        }
        // Change direction periodically
        else if (Time.time >= nextDirectionChangeTime)
        {
            ChooseNewDirection();
        }
    }

    void ChooseNewDirection()
    {
        // Pick a random direction on the horizontal plane
        float randomAngle = Random.Range(0f, 360f);
        targetDirection = new Vector3(
            Mathf.Cos(randomAngle * Mathf.Deg2Rad),
            0f,
            Mathf.Sin(randomAngle * Mathf.Deg2Rad)
        );

        nextDirectionChangeTime = Time.time + Random.Range(changeDirectionInterval * 0.5f, changeDirectionInterval * 1.5f);
    }

    public void SetTargeted(bool targeted)
    {
        isTargeted = targeted;
    }

    public void FinishGame()
    {
        isGameFinished = true;
        
        // On arrête l'animation proprement
        if (animator != null)
        {
            animator.speed = 0;
            isWalking = false;
        }

        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false; 
        }
        
        // On freine le Rigidbody pour qu'il ne glisse pas, mais on le laisse KINEMATIC à false
        // pour qu'il ne passe pas au travers du sol !
        if (rb != null)
        {
            if (rb.isKinematic == false) // <--- LA SÉCURITÉ EST ICI
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.useGravity = false;
                rb.isKinematic = true;
            }
        }
    }
}
