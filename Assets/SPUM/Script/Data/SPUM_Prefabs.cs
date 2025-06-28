using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

public class SPUM_Prefabs : MonoBehaviour
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

    [Header("Movement Settings")]
    public float moveSpeed = 1.3f;
    private Rigidbody2D rb;
    private bool facingRight = true;
    private float horizontal;

    [Header("Jump Settings")]
public float jumpForce = 5f;
public LayerMask groundLayer;
public Transform groundCheck;
public float groundCheckRadius = 0.2f;
    public GameObject prefab;

    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        OverrideControllerInit();
    }

    void Update()
    {
       
        float vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxisRaw("Horizontal");
        // Flip sprite based on input
        if (horizontal > 0 && facingRight) Flip();
        else if (horizontal < 0 && !facingRight) Flip();

        // Update animation state
        if (Mathf.Abs(horizontal) > 0.01f)
        {
            PlayAnimation(PlayerState.MOVE, 0);
        }
        else
        {
            PlayAnimation(PlayerState.IDLE, 0);
        }

        //if (Input.GetKey(KeyCode.LeftShift))
        //{
        //    moveSpeed = 2;
        //}
        //else
        //{
        //    moveSpeed = 1;
        //}

        //// Optional: Apply movement
        //Vector3 direction = new Vector3(horizontal, 0, vertical);
        //transform.Translate(direction * moveSpeed * Time.deltaTime);

    }
    void FixedUpdate()
    {
        rb.velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    public bool allListsHaveItemsExist()
    {
        List<List<AnimationClip>> allLists = new List<List<AnimationClip>>()
    {
        IDLE_List, MOVE_List, ATTACK_List, DAMAGED_List, DEBUFF_List, DEATH_List, OTHER_List
    };

        return allLists.All(list => list.Count > 0);
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
            var stateText = state.ToString();
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
            .SelectMany(package => package.SpumAnimationData)
            .Where(clip => clip.HasData && clip.UnitType == UnitType && clip.index > -1)
            .GroupBy(clip => clip.StateType)
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
            Debug.LogWarning($"Failed to load clip at '{path}'.");
        }
        return clip;
    }
}