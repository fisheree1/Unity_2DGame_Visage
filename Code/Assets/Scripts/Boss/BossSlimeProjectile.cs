using System.Collections;
using UnityEngine;

public class BossSlimeProjectile : MonoBehaviour
{
    private int damage;
    private Vector2 direction;
    private LayerMask targetLayer;
    private float speed;
    private bool isHoming;
    private bool hasHitTarget = false;
    private Transform target;
    private float homingStrength = 2f;
    private float homingUpdateDelay = 0.1f;
    private float lastHomingUpdate = 0f;
    
    // ���
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    
    public void Initialize(int damage, Vector2 direction, LayerMask targetLayer, float speed, bool isHoming = false)
    {
        this.damage = damage;
        this.direction = direction;
        this.targetLayer = targetLayer;
        this.speed = speed;
        this.isHoming = isHoming;
        
        // ��ȡ���
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // �����׷�ٵ�Ļ��Ѱ��Ŀ��
        if (isHoming)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log("BossSlime��Ļ�ѳ�ʼ��׷��Ŀ��");
            }
        }
        
        // ���ó�ʼ�ٶ�
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
        
        // �����鷭ת
        HandleSpriteFlipping(direction);
        
        Debug.Log($"BossSlime��Ļ�ѳ�ʼ�� - �˺�: {damage}, ����: {direction}, ׷��: {isHoming}");
    }
    
    private void HandleSpriteFlipping(Vector2 moveDirection)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = moveDirection.x < 0;
        }
    }
    
    private void Update()
    {
        if (hasHitTarget) return;
        
        // ����׷����Ϊ
        if (isHoming && target != null)
        {
            UpdateHomingBehavior();
        }
        
        // �����ٶȸ��¾��鷭ת
        if (rb != null && spriteRenderer != null)
        {
            spriteRenderer.flipX = rb.velocity.x < 0;
        }
    }
    
    private void UpdateHomingBehavior()
    {
        if (Time.time - lastHomingUpdate < homingUpdateDelay) return;
        
        lastHomingUpdate = Time.time;
        
        // ���㳯��Ŀ��ķ���
        Vector2 targetDirection = (target.position - transform.position).normalized;
        
        // Ӧ��׷��
        if (rb != null)
        {
            Vector2 currentVelocity = rb.velocity;
            Vector2 newVelocity = Vector2.Lerp(currentVelocity.normalized, targetDirection, homingStrength * Time.deltaTime);
            rb.velocity = newVelocity * speed;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHitTarget) return;
        
        Debug.Log($"BossSlime��Ļ����: {collision.name}, �㼶: {collision.gameObject.layer}, ��ǩ: {collision.tag}");
        
        // ����Ƿ�������
        bool hitPlayer = false;
        
        // ���㼶���
        if (((1 << collision.gameObject.layer) & targetLayer) != 0)
        {
            hitPlayer = true;
        }
        
        // ����ǩ��飨���ã�
        if (collision.CompareTag("Player"))
        {
            hitPlayer = true;
        }
        
        if (hitPlayer)
        {
            // ���������˺�
            HeroLife playerLife = collision.GetComponent<HeroLife>();
            if (playerLife != null)
            {
                playerLife.TakeDamage(damage);
                hasHitTarget = true;
                Debug.Log($"BossSlime��Ļ���������� {damage} ���˺�");
                
                // ����������Ч
                CreateHitEffect();
            }
            else
            {
                Debug.LogWarning("��������ҵ�δ�ҵ�HeroLife�����");
            }
            
            // ���ٵ�Ļ
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Ground") || collision.CompareTag("Wall") || collision.CompareTag("Platform"))
        {
            // ���л���
            Debug.Log("BossSlime��Ļ���л�������������");
            Destroy(gameObject);
        }
    }
    
    private void CreateHitEffect()
    {
        // �����򵥵Ļ�����Ч
        GameObject hitEffect = new GameObject("ProjectileHitEffect");
        hitEffect.transform.position = transform.position;
        
        // �����Ҫ�������������������ϵͳ������Ч
        // ����ֻ�Ǵ���һ���򵥵���ɢԲ��
        SpriteRenderer effectSprite = hitEffect.AddComponent<SpriteRenderer>();
        
        // �����򵥵�Բ������
        Texture2D texture = new Texture2D(32, 32);
        Color[] colors = new Color[32 * 32];
        Vector2 center = new Vector2(16, 16);
        
        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= 15)
                {
                    colors[y * 32 + x] = new Color(1f, 0.5f, 0f, 0.8f); // ��ɫ
                }
                else
                {
                    colors[y * 32 + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        Sprite effectSprite2 = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        effectSprite.sprite = effectSprite2;
        effectSprite.sortingOrder = 10;
        
        // ����������Ч
        StartCoroutine(AnimateHitEffect(hitEffect));
    }
    
    private IEnumerator AnimateHitEffect(GameObject effect)
    {
        float duration = 0.3f;
        float elapsedTime = 0f;
        Vector3 startScale = Vector3.one * 0.5f;
        Vector3 endScale = Vector3.one * 1.5f;
        
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            effect.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            
            SpriteRenderer sr = effect.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color color = sr.color;
                color.a = 1f - t;
                sr.color = color;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        Destroy(effect);
    }
    
    private void OnBecameInvisible()
    {
        // ��Ļ�뿪��Ļʱ����
        Destroy(gameObject);
    }
}