using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class NPCScript : MonoBehaviour
{
    public float walkingSpeed = 2f; // Walking pace speed
    public float waitTime = 2f; // Time to wait at each patrol point
    public List<Transform> patrolPoints = new List<Transform>();
    
    private Animator anim;
    private NavMeshAgent kunoichiAgent;
    private Transform kunoichiTransform;
    private int currentPosition;
    private bool isWaiting = false;

    void Start()
    {
        kunoichiAgent = GetComponent<NavMeshAgent>();
        kunoichiTransform = GetComponent<Transform>();
        anim = GetComponent<Animator>(); // Assign the Animator component

        // Set walking speed
        kunoichiAgent.speed = walkingSpeed;

        // Make sure the rotation of the agent is controlled manually
        kunoichiAgent.updateRotation = false;

        // Set the destination to the first patrol point
        kunoichiAgent.destination = patrolPoints[currentPosition].position;
    }

    void Update()
    {
        // Only update the destination if not waiting
        if (!isWaiting)
        {
            kunoichiAgent.destination = patrolPoints[currentPosition].position;

            // Set animation parameter for walking based on movement speed
            anim.SetFloat("Speed", kunoichiAgent.velocity.magnitude); // Using NavMeshAgent's velocity

            // Check if the agent is moving and adjust the character's facing direction
            Vector3 direction = kunoichiAgent.velocity;

            if (direction.x > 0)
            {
                // Face right
                kunoichiTransform.localScale = new Vector3(1, 1, 1);
            }
            else if (direction.x < 0)
            {
                // Face left
                kunoichiTransform.localScale = new Vector3(-1, 1, 1);
            }

            // Move to the next patrol point when close to the current one
            if (!kunoichiAgent.pathPending && kunoichiAgent.remainingDistance < 0.5f)
            {
                StartCoroutine(WaitAtPatrolPoint()); // Start waiting coroutine
            }
        }
        else
        {
            anim.SetFloat("Speed", 0); // Set speed to 0 when waiting
        }
    }

    // Coroutine to wait at the patrol point for the specified wait time
    IEnumerator WaitAtPatrolPoint()
    {
        isWaiting = true; // Prevent movement during waiting

        // Wait for the set amount of time
        yield return new WaitForSeconds(waitTime);

        // Move to the next patrol point
        currentPosition++;
        if (currentPosition >= patrolPoints.Count)
        {
            currentPosition = 0;
        }

        isWaiting = false; // Allow movement again
    }
}
