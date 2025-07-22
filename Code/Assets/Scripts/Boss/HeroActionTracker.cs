using UnityEngine;

/// <summary>
/// Ӣ�۶���׷����
/// ���ڼ����ҵĶ�����֪ͨBoss���з�Ӧ
/// </summary>
public class HeroActionTracker : MonoBehaviour
{
    private BossDemon bossDemon;
    private HeroMovement heroMovement;
    private HeroAttackController heroAttack;
    private Transform player;
    
    // ׷�ٱ���
    private bool heroRolledThisFrame = false;
    private bool heroJumpedThisFrame = false;
    private bool heroAttackedThisFrame = false;
    
    // ��һ֡״̬
    private bool wasGroundedLastFrame = false;
    private bool wasAttackingLastFrame = false;
    private bool wasSlidingLastFrame = false;
    
    // ʱ�����
    private float lastRollTime = 0f;
    private float lastJumpTime = 0f;
    private float lastAttackTime = 0f;
    private float actionCooldown = 0.5f; // ��ֹ�����������
    
    /// <summary>
    /// ��ʼ��׷����
    /// </summary>
    /// <param name="demon">Boss��ħ����</param>
    public void Initialize(BossDemon demon)
    {
        bossDemon = demon;
        
        // ����Ӣ������
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            heroMovement = playerObj.GetComponent<HeroMovement>();
            heroAttack = playerObj.GetComponent<HeroAttackController>();
        }
        
        if (heroMovement != null)
        {
            wasGroundedLastFrame = heroMovement.IsGrounded();
            wasSlidingLastFrame = heroMovement.getIsSliding();
        }
        
        if (heroAttack != null)
        {
            wasAttackingLastFrame = heroAttack.IsAttacking;
        }
    }
    
    void Update()
    {
        if (bossDemon == null || bossDemon.IsDead) return;
        if (heroMovement == null || heroAttack == null) return;
        
        // ����֡��־
        heroRolledThisFrame = false;
        heroJumpedThisFrame = false;
        heroAttackedThisFrame = false;
        
        // ���Ӣ�۶���
        CheckForRoll();
        CheckForJump();
        CheckForAttack();
        
        // ������һ֡״̬
        wasGroundedLastFrame = heroMovement.IsGrounded();
        wasAttackingLastFrame = heroAttack.IsAttacking;
        wasSlidingLastFrame = heroMovement.getIsSliding();
    }
    
    /// <summary>
    /// ��鷭������
    /// </summary>
    private void CheckForRoll()
    {
        // ���Ӣ�ۿ�ʼ���У�������
        bool isCurrentlySliding = heroMovement.getIsSliding();
        
        if (isCurrentlySliding && !wasSlidingLastFrame)
        {
            // Ӣ�۸տ�ʼ����/����
            if (Time.time - lastRollTime > actionCooldown)
            {
                heroRolledThisFrame = true;
                lastRollTime = Time.time;
                Debug.Log("��⵽Ӣ�۶���: ����/����");
            }
        }
    }
    
    /// <summary>
    /// �����Ծ����
    /// </summary>
    private void CheckForJump()
    {
        // ���Ӣ���뿪���棨��Ծ��
        bool isCurrentlyGrounded = heroMovement.IsGrounded();
        
        if (wasGroundedLastFrame && !isCurrentlyGrounded && heroMovement.GetVelocityY() > 0)
        {
            // Ӣ�۸��뿪���沢�������ٶȣ���Ծ��
            if (Time.time - lastJumpTime > actionCooldown)
            {
                heroJumpedThisFrame = true;
                lastJumpTime = Time.time;
                Debug.Log("��⵽Ӣ�۶���: ��Ծ");
            }
        }
    }
    
    /// <summary>
    /// ��鹥������
    /// </summary>
    private void CheckForAttack()
    {
        // ���Ӣ�ۿ�ʼ����
        bool isCurrentlyAttacking = heroAttack.IsAttacking;
        
        if (isCurrentlyAttacking && !wasAttackingLastFrame)
        {
            // Ӣ�۸տ�ʼ����
            if (Time.time - lastAttackTime > actionCooldown)
            {
                heroAttackedThisFrame = true;
                lastAttackTime = Time.time;
                
                // ֪ͨBoss��ħ������������׷��
                if (bossDemon != null)
                {
                    bossDemon.OnHeroAttack();
                }
                
                Debug.Log("��⵽Ӣ�۶���: ����");
            }
        }
    }
    
    /// <summary>
    /// ���Ӣ���Ƿ��ڱ�֡����
    /// </summary>
    /// <returns>�����������true</returns>
    public bool HasHeroRolled()
    {
        return heroRolledThisFrame;
    }
    
    /// <summary>
    /// ���Ӣ���Ƿ��ڱ�֡��Ծ
    /// </summary>
    /// <returns>�����Ծ����true</returns>
    public bool HasHeroJumped()
    {
        return heroJumpedThisFrame;
    }
    
    /// <summary>
    /// ���Ӣ���Ƿ��ڱ�֡����
    /// </summary>
    /// <returns>�����������true</returns>
    public bool HasHeroAttacked()
    {
        return heroAttackedThisFrame;
    }
    
    /// <summary>
    /// ��ȡ�����ϴη�����ʱ��
    /// </summary>
    /// <returns>ʱ����</returns>
    public float TimeSinceLastRoll()
    {
        return Time.time - lastRollTime;
    }
    
    /// <summary>
    /// ��ȡ�����ϴ���Ծ��ʱ��
    /// </summary>
    /// <returns>ʱ����</returns>
    public float TimeSinceLastJump()
    {
        return Time.time - lastJumpTime;
    }
    
    /// <summary>
    /// ��ȡ�����ϴι�����ʱ��
    /// </summary>
    /// <returns>ʱ����</returns>
    public float TimeSinceLastAttack()
    {
        return Time.time - lastAttackTime;
    }
    
    /// <summary>
    /// ���Ӣ�۵�ǰ�Ƿ����ڷ���
    /// </summary>
    /// <returns>������ڷ�������true</returns>
    public bool IsHeroRolling()
    {
        return heroMovement != null && heroMovement.getIsSliding();
    }
    
    /// <summary>
    /// ���Ӣ�۵�ǰ�Ƿ�������Ծ
    /// </summary>
    /// <returns>���������Ծ����true</returns>
    public bool IsHeroJumping()
    {
        return heroMovement != null && !heroMovement.IsGrounded() && heroMovement.GetVelocityY() > 0;
    }
    
    /// <summary>
    /// ���Ӣ�۵�ǰ�Ƿ����ڹ���
    /// </summary>
    /// <returns>������ڹ�������true</returns>
    public bool IsHeroAttacking()
    {
        return heroAttack != null && heroAttack.IsAttacking;
    }
    
    /// <summary>
    /// ��ȡ��ǰ��������
    /// </summary>
    /// <returns>��������</returns>
    public HeroAttackController.AttackType GetCurrentAttackType()
    {
        return heroAttack != null ? heroAttack.CurrentAttackType : HeroAttackController.AttackType.None;
    }
}