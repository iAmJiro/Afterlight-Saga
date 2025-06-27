using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyFollow : MonoBehaviour
{
    public Transform player;
    public float followDistance = 5f;
    public float stopDistance = 2f;
    public float moveSpeed = 2f;

    private bool facingRight = true;
    private Rigidbody2D rb;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(player.position, transform.position);
        if (distanceToPlayer <= followDistance && distanceToPlayer > stopDistance)
        {
            FollowPlayer();
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
        animator.SetFloat("xVelocity", Mathf.Abs(rb.velocity.x));
        FlipIfNeeded();
    }

    void FollowPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed, direction.y * moveSpeed);
        
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
}