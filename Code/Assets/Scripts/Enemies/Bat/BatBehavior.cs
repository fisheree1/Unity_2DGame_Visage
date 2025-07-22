using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

public class BatBehavior : MonoBehaviour
{
    [Header("Damage Effects")]
    [SerializeField] private float damageFlashDuration = 0.2f;
    [SerializeField] private Color damageFlashColor = Color.red;
    public float Movespeed;
    public float Chasespeed;
    public float radius;
    public float startWaitTime;
    private float waitTime;
    public int health = 2;
    public int damage = 1;
    private bool isDead = false;
    private SpriteRenderer spriteRenderer=null;
    private Color originalColor=Color.white;
    public Transform attackPoint;
    public float attackArea;


    public Transform player;
    private HeroMovement heroMovement;
    public Transform movePos;
    public Transform leftDownPos;
    public Transform rightUpPos;

    public GameObject Tipl;

    void Start()
    {
        waitTime = startWaitTime;
        if (movePos != null)
            movePos.position = GetRandomPos();

        heroMovement = player.GetComponent<HeroMovement>();
    }

    void Update()
    {
        if (player == null || movePos == null || leftDownPos == null || rightUpPos == null)
            return;

        // ����׷�����
        if (heroMovement.IsHiding == false && Vector2.Distance(transform.position, player.position) < radius)
        {
            ChasePlayer(player.position);
            Tipl.SetActive(false);
        }
        else
        {
            Patrol();
            Tipl.SetActive(true);
        }
        if(heroMovement.IsHiding == false)
            AttackArea();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.CompareTag("PlayerAttack"))
        {
            int damage = other.GetComponent<AttackHitbox>().damage;
            TakeDamage(damage);

        }

    }

    void Patrol()
    {
        LookTowards(movePos.position);
        transform.position = Vector2.MoveTowards(transform.position, movePos.position, Movespeed * Time.deltaTime);
        if (Vector2.Distance(transform.position, movePos.position) < 0.1f)
        {
            if (waitTime <= 0)
            {
                movePos.position = GetRandomPos();
                waitTime = startWaitTime;
            }
            else
            {
                waitTime -= Time.deltaTime;
            }
        }
    }

    Vector2 GetRandomPos()
    {
        return new Vector2(
            Random.Range(leftDownPos.position.x, rightUpPos.position.x),
            Random.Range(leftDownPos.position.y, rightUpPos.position.y)
        );
    }

    public void LookTowards(Vector2 targetPos)
    {
        Vector2 direction = targetPos - (Vector2)transform.position;
        if (direction == Vector2.zero) return;
        if (direction.x > 0)
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        else
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    public void ChasePlayer(Vector2 playerPos)
    {
        LookTowards(playerPos);
        Vector2 direction = playerPos - (Vector2)transform.position;
         
        transform.position = Vector2.MoveTowards(transform.position, playerPos, Chasespeed * Time.deltaTime);
    }
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        // Apply damage
        health = Mathf.Max(0, health - damageAmount);

        // Trigger damage effect
        if (spriteRenderer != null)
        {
            StopCoroutine(nameof(DamageFlashCoroutine));
            StartCoroutine(DamageFlashCoroutine());
        }

        // Check for death
        if (health <= 0 && !isDead)
        {
            isDead = true;
            StartCoroutine(Die());
        }
    }

    private IEnumerator DamageFlashCoroutine()
    {
        if (spriteRenderer == null) yield break;

        spriteRenderer.color = damageFlashColor;
        yield return new WaitForSeconds(damageFlashDuration);

        // Restore color if still alive
        if (!isDead)
        {
            spriteRenderer.color = originalColor;
        }
    }

    private IEnumerator Die()
    {
        Debug.Log(gameObject.name + " died!");

        // Stop movement
        Movespeed = 0;
        Chasespeed = 0;

        // Visual death effect
        if (spriteRenderer != null)
        {
            spriteRenderer.color = damageFlashColor;
            yield return new WaitForSeconds(0.5f);
        }

        // Destroy object
        Destroy(gameObject);
    }
    void AttackArea()
    {
                Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackArea);
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Player") && heroMovement.IsHiding == false)
            {
                enemy.GetComponent<HeroLife>().TakeDamage(damage);
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(attackPoint.position, attackArea);
    }
}

