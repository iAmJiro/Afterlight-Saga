using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

public class NPCScript : MonoBehaviour
{
    public enum PlayerState
    {
        IDLE,
        MOVE,
        ATTACK,
        DAMAGED,
        DEBUFF,
        DEATH,
        OTHER,
    }

    [Header("SPUM Setup")]
    public Animator _anim;
    private AnimatorOverrideController OverrideController;

    public string UnitType;
    public List<SpumPackage> spumPackages = new();
    public List<SPUM_AnimationData> SpumAnimationData = new();
    public Dictionary<string, List<AnimationClip>> StateAnimationPairs = new();
    public List<AnimationClip> IDLE_List = new();
    public List<AnimationClip> MOVE_List = new();
    public List<AnimationClip> ATTACK_List = new();
    public List<AnimationClip> DAMAGED_List = new();
    public List<AnimationClip> DEBUFF_List = new();
    public List<AnimationClip> DEATH_List = new();
    public List<AnimationClip> OTHER_List = new();

    [Header("Patrol Settings")]
    public float walkingSpeed = 2f;
    public float waitTime = 2f;
    public List<Transform> patrolPoints = new List<Transform>();

    [Header("Movement Settings")]
    private float idleCooldown = 0.2f;
    private float idleTimer = 0f;
    private NavMeshAgent agent;
    private int currentPatrolIndex;
    private bool isWaiting;
    private bool facingRight = true;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (_anim == null) _anim = GetComponentInChildren<Animator>();
        if (_anim == null)
        {
            Debug.LogError("Animator not found on NPC.");
            return;
        }

        OverrideControllerInit();

        agent.speed = walkingSpeed;
        agent.updateRotation = false;
        if (patrolPoints.Count > 0)
            agent.destination = patrolPoints[currentPatrolIndex].position;
    }

    void Update()
    {

        Vector3 velocity = agent.velocity;
        bool currentlyMoving = velocity.sqrMagnitude > 0.05f;
        bool isMoving = velocity.sqrMagnitude > 0.01f;

        if (currentlyMoving)
        {
            idleTimer = 0f;
            PlayAnimation(PlayerState.MOVE, 0);

            if (velocity.x > 0 && facingRight) Flip();
            else if (velocity.x < 0 && !facingRight) Flip();
        }
        else
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleCooldown)
                PlayAnimation(PlayerState.IDLE, 0);
        }
        if (isWaiting || patrolPoints.Count == 0)
        {
            PlayAnimation(PlayerState.IDLE, 0);
            return;
        }

        
        //if (isMoving)
        //{
        //    _anim.Play("MOVE");
        //    PlayAnimation(PlayerState.MOVE, 0);
        //    if (velocity.x > 0 && !facingRight) Flip();
        //    else if (velocity.x < 0 && facingRight) Flip();
        //}
        else
        {
            PlayAnimation(PlayerState.IDLE, 0);
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            StartCoroutine(WaitAtPatrolPoint());
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    IEnumerator WaitAtPatrolPoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
        agent.destination = patrolPoints[currentPatrolIndex].position;
        isWaiting = false;
    }

    public void OverrideControllerInit()
    {
        OverrideController = new AnimatorOverrideController();
        OverrideController.runtimeAnimatorController = _anim.runtimeAnimatorController;

        foreach (var clip in _anim.runtimeAnimatorController.animationClips)
        {
            OverrideController[clip.name] = clip;
        }

        _anim.runtimeAnimatorController = OverrideController;

        foreach (PlayerState state in System.Enum.GetValues(typeof(PlayerState)))
        {
            StateAnimationPairs[state.ToString()] = state switch
            {
                PlayerState.IDLE => IDLE_List,
                PlayerState.MOVE => MOVE_List,
                PlayerState.ATTACK => ATTACK_List,
                PlayerState.DAMAGED => DAMAGED_List,
                PlayerState.DEBUFF => DEBUFF_List,
                PlayerState.DEATH => DEATH_List,
                _ => OTHER_List
            };
        }

        PopulateAnimationLists();
    }

    public void PopulateAnimationLists()
    {
        IDLE_List.Clear(); MOVE_List.Clear(); ATTACK_List.Clear();
        DAMAGED_List.Clear(); DEBUFF_List.Clear(); DEATH_List.Clear(); OTHER_List.Clear();

        var groupedClips = spumPackages
            .SelectMany(pkg => pkg.SpumAnimationData)
            .Where(c => c.HasData && c.UnitType == UnitType && c.index > -1)
            .GroupBy(c => c.StateType)
            .ToDictionary(g => g.Key, g => g.OrderBy(c => c.index).ToList());

        foreach (var kvp in groupedClips)
        {
            var list = kvp.Key switch
            {
                "IDLE" => IDLE_List,
                "MOVE" => MOVE_List,
                "ATTACK" => ATTACK_List,
                "DAMAGED" => DAMAGED_List,
                "DEBUFF" => DEBUFF_List,
                "DEATH" => DEATH_List,
                _ => OTHER_List
            };

            list.AddRange(kvp.Value.Select(c => LoadAnimationClip(c.ClipPath)));
        }
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
            Debug.LogWarning($"Failed to load clip at '{path}'");
        }
        return clip;
    }
}