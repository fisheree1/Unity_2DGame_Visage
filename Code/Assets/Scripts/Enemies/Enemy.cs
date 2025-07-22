using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed;
    private Vector2 direction;
    private bool isHit;
    private AnimatorStateInfo info;
    private Animator animator;
    private Animator hitanimation;

    new private Rigidbody2D rigidbody;
    
    // Check if this enemy has a state machine (like SlimeP, SGoBlinP, etc.)
    private bool hasStateMachine = false;
    
    void Start()
    {
        animator = transform.GetComponent<Animator>();
        hitanimation = transform.GetChild(3).GetComponent<Animator>();
        rigidbody = transform.GetComponent<Rigidbody2D>();
        
        // Check if this enemy uses a state machine
        // State machine enemies should handle their own movement
        hasStateMachine = GetComponent<SlimeP>() != null || 
                         GetComponent<SGoBlinP>() != null ||
                         GetComponent<MonoBehaviour>().GetType().Name.EndsWith("P");
    }


    void Update()
    {
        // Only handle movement if this enemy doesn't have a state machine
        if (!hasStateMachine)
        {
            info = animator.GetCurrentAnimatorStateInfo(0);
            if (isHit)
            {
                Debug.Log("Hit �����ƶ���");
                rigidbody.velocity = direction * speed;
                if (info.normalizedTime >= 0.3f)
                {
                    Debug.Log("Hit �����ƶ����");
                    isHit = false;
                }
            }
        }
    }
    
    public void GetHit(Vector2 direction)
    {
        transform.localScale = new Vector3(direction.x, 1, 1);
        
        // Only set isHit for non-state machine enemies
        // State machine enemies handle their own hit logic
        if (!hasStateMachine)
        {
            isHit = true;
            this.direction = direction;
        }
        
        animator.SetTrigger("Hit");
        hitanimation.SetTrigger("Hit");
    }
}
