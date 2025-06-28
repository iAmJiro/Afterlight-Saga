using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyFollow : MonoBehaviour
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

    [Header("Follow Settings")]
    public Transform player;
    public float followDistance = 5f;
    public float stopDistance = 2f;
    public float moveSpeed = 2f;

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

    private bool facingRight = true;
    private Rigidbody2D rb;
    private float previousSpeed = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (_anim == null)
            _anim = GetComponentInChildren<Animator>();

        if (_anim != null)
        {
            OverrideController = new AnimatorOverrideController(_anim.runtimeAnimatorController);
            _anim.runtimeAnimatorController = OverrideController;
        }

        // Initialize animation state dictionary
        StateAnimationPairs["IDLE"] = IDLE_List;
        StateAnimationPairs["MOVE"] = MOVE_List;
        StateAnimationPairs["ATTACK"] = ATTACK_List;
        StateAnimationPairs["DAMAGED"] = DAMAGED_List;
        StateAnimationPairs["DEBUFF"] = DEBUFF_List;
        StateAnimationPairs["DEATH"] = DEATH_List;
    }

    void Update()
    {
        if (player == null || _anim == null)
            return;

        float distanceToPlayer = Vector3.Distance(player.position, transform.position);

        if (distanceToPlayer <= followDistance && distanceToPlayer > stopDistance)
        {
            FollowPlayer();
        }
        else
        {
            rb.velocity = Vector2.zero;
        }

        FlipIfNeeded();

        float currentSpeed = rb.velocity.magnitude;

        if (currentSpeed > previousSpeed)
        {
            PlayAnimation(PlayerState.MOVE, 0);
        }
        else if (currentSpeed < 0.05f)
        {
            PlayAnimation(PlayerState.IDLE, 0);
        }

        previousSpeed = currentSpeed;
    }

    void FollowPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * moveSpeed;
    }

    void FlipIfNeeded()
    {
        if (player.position.x < transform.position.x && facingRight)
        {
            Flip();
        }
        else if (player.position.x > transform.position.x && !facingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
        facingRight = !facingRight;
    }

    public void PlayAnimation(PlayerState state, int index)
    {
        if (_anim == null || !StateAnimationPairs.TryGetValue(state.ToString(), out var list) || list.Count <= index)
            return;

        if (OverrideController != null)
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
            Debug.LogWarning($"Failed to load animation at: {path}");
        return clip;
    }
}