using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class NPCScript : MonoBehaviour
{
    [Header("SPUM Setup")]
    public float _version;
    public bool EditChk;
    public string _code;
    public Animator _anim;
    private AnimatorOverrideController OverrideController;

    public string UnitType;
    public List<SpumPackage> spumPackages = new();
    public List<PreviewMatchingElement> ImageElement = new();
    public List<SPUM_AnimationData> SpumAnimationData = new();
    public Dictionary<string, List<AnimationClip>> StateAnimationPairs = new();
    public List<AnimationClip> IDLE_List = new();
    public List<AnimationClip> MOVE_List = new();
    public List<AnimationClip> ATTACK_List = new();
    public List<AnimationClip> DAMAGED_List = new();
    public List<AnimationClip> DEBUFF_List = new();
    public List<AnimationClip> DEATH_List = new();
    public List<AnimationClip> OTHER_List = new();

    public float walkingSpeed = 2f; // Walking pace speed
    public float waitTime = 2f; // Time to wait at each patrol point
    public List<Transform> patrolPoints = new List<Transform>();
    
    private NavMeshAgent kunoichiAgent;
    private Transform kunoichiTransform;
    private int currentPosition;
    private bool isWaiting = false;

    private bool facingRight = true;
    private float horizontal;
    private float vertical;

    void Start()
    {
        kunoichiAgent = GetComponent<NavMeshAgent>();
        kunoichiTransform = GetComponent<Transform>();

        // Set walking speed
        kunoichiAgent.speed = walkingSpeed;

        // Make sure the rotation of the agent is controlled manually
        kunoichiAgent.updateRotation = false;

        // Set the destination to the first patrol point
        kunoichiAgent.destination = patrolPoints[currentPosition].position;
    }

    void Update()
    {
        if (!isWaiting)
        {
            // Move to current patrol destination
            kunoichiAgent.destination = patrolPoints[currentPosition].position;

            // Check if the agent is actually moving
            Vector3 velocity = kunoichiAgent.velocity;

            if (velocity.sqrMagnitude > 0.01f)
            {
                PlayAnimation(PlayerState.MOVE, 0);

                // Optional: Flip based on direction.x
                if (velocity.x > 0 && !facingRight) Flip();
                else if (velocity.x < 0 && facingRight) Flip();
            }
            else
            {
                PlayAnimation(PlayerState.IDLE, 0);
            }

            // Proceed to the next patrol point if close enough
            if (!kunoichiAgent.pathPending && kunoichiAgent.remainingDistance < 0.5f)
            {
                StartCoroutine(WaitAtPatrolPoint());
            }
        }
    }
    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
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
    public void PlayAnimation(PlayerState state, int index)
    {
        if (!StateAnimationPairs.TryGetValue(state.ToString(), out var list) || list.Count <= index) return;

        OverrideController[state.ToString()] = list[index];

        _anim.SetBool("1_Move", state == PlayerState.MOVE);
        _anim.SetBool("5_Debuff", state == PlayerState.DEBUFF);
        _anim.SetBool("isDeath", state == PlayerState.DEATH);

        if (state != PlayerState.MOVE && state != PlayerState.DEBUFF)
        {
            foreach (var param in _anim.parameters)
            {
                if (param.type == AnimatorControllerParameterType.Trigger &&
                    param.name.ToUpper().Contains(state.ToString().ToUpper()))
                {
                    _anim.SetTrigger(param.name);
                }
            }
        }
    }

    AnimationClip LoadAnimationClip(string path)
    {
        var clip = Resources.Load<AnimationClip>(path.Replace(".anim", ""));
        if (clip == null)
        {
            Debug.LogWarning($"Failed to load clip at '{path}'.");
        }
        return clip;
    }
}
