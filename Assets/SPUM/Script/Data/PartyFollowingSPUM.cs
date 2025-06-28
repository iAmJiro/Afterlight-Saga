using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class PartyFollowingSPUM : MonoBehaviour
{
    public enum PlayerState
    {
        IDLE,
        MOVE,
        ATTACK,
        DAMAGED,
        DEBUFF,
        DEATH,
    }

        [Header("SPUM Setup")]
    public float _version;
    public bool EditChk;
    public string _code;
    [SerializeField] private Animator _anim;
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

    [Header("Follow Settings")]
    public Transform playerToFollow;
    public float followDistance = 5f;
    public float stopDistance = 2f;
    public float moveSpeed = 2f;

    private bool facingRight = true;
    private Rigidbody2D rb;
    private float previousSpeed = 0f;
    public GameObject prefab;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        OverrideControllerInit();
        if (_anim == null)
            _anim = GetComponent<Animator>();
        

    }

    void Update()
    {

        if (playerToFollow == null)
        {
            rb.velocity = Vector2.zero;
            PlayAnimation(PlayerState.IDLE, 0);
            return;
        }
        else 
        {
            PlayAnimation(PlayerState.MOVE, 0);

        }

        float distance = Vector3.Distance(playerToFollow.position, transform.position);

        if (distance <= followDistance && distance > stopDistance)
        {
            Vector2 direction = (playerToFollow.position - transform.position).normalized;
            rb.velocity = direction * moveSpeed;
            PlayAnimation(PlayerState.MOVE, 0);
        }
        else
        {
            rb.velocity = Vector2.zero;
            PlayAnimation(PlayerState.IDLE, 0);
        }

        FlipIfNeeded();
        _anim.SetFloat("xVelocity", Mathf.Abs(rb.velocity.x));
        float currentSpeed = rb.velocity.magnitude;

        if (currentSpeed > previousSpeed)
        {
            PlayAnimation(PlayerState.MOVE, 0);
            Debug.Log("Velocity is increasing");
        }
        else if (currentSpeed < previousSpeed)
        {
            PlayAnimation(PlayerState.IDLE, 0);
            Debug.Log("Velocity is decreasing");
        }

        previousSpeed = currentSpeed;


    }

    void FlipIfNeeded()
    {
        if (playerToFollow == null) return;

        if (playerToFollow.position.x < transform.position.x && facingRight)
        {
            Flip();
        }
        else if (playerToFollow.position.x > transform.position.x && !facingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        facingRight = !facingRight;
    }

    public void OverrideControllerInit()
    {
        OverrideController = new AnimatorOverrideController();
        OverrideController.runtimeAnimatorController = _anim.runtimeAnimatorController;

        foreach (AnimationClip clip in _anim.runtimeAnimatorController.animationClips)
        {
            OverrideController[clip.name] = clip;
        }

        _anim.runtimeAnimatorController = OverrideController;

        foreach (PlayerState state in Enum.GetValues(typeof(PlayerState)))
        {
            string stateText = state.ToString();
            StateAnimationPairs[stateText] = state switch
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

    [ContextMenu("PopulateAnimationLists")]
    public void PopulateAnimationLists()
    {
        IDLE_List.Clear(); MOVE_List.Clear(); ATTACK_List.Clear();
        DAMAGED_List.Clear(); DEBUFF_List.Clear(); DEATH_List.Clear(); OTHER_List.Clear();

        var groupedClips = spumPackages
            .SelectMany(p => p.SpumAnimationData)
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
            list.AddRange(kvp.Value.Select(clip => LoadAnimationClip(clip.ClipPath)));
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
            Debug.LogWarning($"Failed to load animation at: {path}");
        }
        return clip;
    }
}