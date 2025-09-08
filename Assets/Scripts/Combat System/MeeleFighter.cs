using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackStates{ Idle, Windup, Imapct, Cooldown}


public class MeeleFighter : MonoBehaviour
{
    [SerializeField] List<AttackData> attacks;
    [SerializeField] List<AttackData> longRangeAttacks;
    [SerializeField] float longRangeAttackThreshold = 1.5f;
    [SerializeField] GameObject sword;

    [SerializeField] float rotationSpeed = 500f;

    public bool IsTakingHit { get; private set; } = false;

    // 委托
    public event Action<MeeleFighter> OnGoHit;
    public event Action OnHitComplete;

    BoxCollider swordCollider;
    SphereCollider leftHandCollider, rightHandCollider, leftFootCollider, rightFootCollider;
    
    Animator animator;

    public AttackStates AttackState { get; private set; }

    bool doCombo;
    int comboCount = 0;

    //将其设置为一个属性，并将set设为私人，使其他类不能随便改变(属性要以大写字母开头)
    public bool InAction { get; private set; } = false;  //是否正在攻击

    public bool InCounter { get; set; }

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (sword != null)
        {
            swordCollider = sword.GetComponent<BoxCollider>();

            leftHandCollider = animator.GetBoneTransform(HumanBodyBones.LeftHand).GetComponent<SphereCollider>();
            rightHandCollider = animator.GetBoneTransform(HumanBodyBones.RightHand).GetComponent<SphereCollider>();
            leftFootCollider = animator.GetBoneTransform(HumanBodyBones.LeftFoot).GetComponent<SphereCollider>();
            rightFootCollider = animator.GetBoneTransform(HumanBodyBones.RightFoot).GetComponent<SphereCollider>();

            DisableAllHitBoxes();
        }
    }

    public void TryToAttack(MeeleFighter target = null)
    {
        if (!InAction)
        {
            StartCoroutine(Attack(target));
        }
        else if (AttackState == AttackStates.Imapct || AttackState == AttackStates.Cooldown)
        {
            doCombo = true;
        }
    }

    IEnumerator Attack(MeeleFighter target = null)
    {
        InAction = true;
        AttackState = AttackStates.Windup;

        var attack = attacks[comboCount];

        var attackDir = transform.forward;
        Vector3 startPos = transform.position;
        Vector3 targetPos = Vector3.zero;
        if(target != null)
        {
            var vecToTarget = target.transform.position - transform.position;
            vecToTarget.y = 0;
            
            attackDir = vecToTarget.normalized;
            float distance = vecToTarget.magnitude - attack.DistanceFromTarget;

            // 如果目标敌人较远，设置攻击为远程攻击
            if (distance > longRangeAttackThreshold && longRangeAttacks.Count > 0)
            {
                attack = longRangeAttacks[0];
            }

            if (attack.MoveToTarget)
            {
                if (distance <= attack.MaxMoveDistance)
                    targetPos = target.transform.position - attackDir * attack.DistanceFromTarget;
                else
                    targetPos = startPos + attackDir * attack.MaxMoveDistance;
            }
        }

        //动画过度函数：占原动画20%时，过度到下一个动画
        animator.CrossFade(attack.AnimName, 0.2f);
        //当调用 animator.CrossFade("Slash", 0.2f) 时，
        //动画系统并不会立即切换到 "Slash" 状态，而是需要一帧的时间来处理过渡逻辑：
        yield return null;

        //获取下一个的动画信息(因为当前使用的是淡出动画函数，过渡到下一个)
        var animState = animator.GetNextAnimatorStateInfo(1);

        float timer = 0f;
        while (timer <= animState.length)
        {
            if (IsTakingHit) break;

            timer += Time.deltaTime;
            float normalizedTime = timer / animState.length;

            if(target != null && attack.MoveToTarget)
            {
                float percTime = (normalizedTime - attack.MoveStartTime) / (attack.MoveEndTime - attack.MoveStartTime);
                transform.position = Vector3.Lerp(startPos, targetPos, percTime);
            }

            //攻击时旋转至摄像头的方向
            if(attackDir != null)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, Quaternion.LookRotation(attackDir), rotationSpeed * Time.deltaTime);
            }

            if (AttackState == AttackStates.Windup)
            {
                if (InCounter) break;

                if(normalizedTime >= attack.ImpactStartTime)
                {
                    AttackState = AttackStates.Imapct;
                    //打开触发器
                    EnableHitBox(attack);
                }
            }else if (AttackState == AttackStates.Imapct)
            {
                if (normalizedTime >= attack.ImpactEndTime)
                {
                    AttackState = AttackStates.Cooldown;
                    //关闭触发器
                    DisableAllHitBoxes();
                }
            }
            else if (AttackState == AttackStates.Cooldown)
            {
                if (doCombo)
                {
                    doCombo = false;
                    //当连击技术次 == 列表攻击长度时，模为0
                    comboCount = (comboCount + 1) % attacks.Count;

                    StartCoroutine(Attack());

                    //退出协程
                    yield break;
                }
            }

            //暂时等待一帧后执行下一行函数
            yield return null;
        }

        AttackState = AttackStates.Idle;
        comboCount = 0;
        InAction = false;
        //currTarget = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Hitbox" && !IsTakingHit && !InCounter)
        {
            var attacker = other.GetComponentInParent<MeeleFighter>();
            //if (attacker.currTarget != this)
            //    return;

            Debug.Log("角色受伤");
            StartCoroutine(PlayerHitReaction(attacker));
        }
    }

    IEnumerator PlayerHitReaction(MeeleFighter attacker)
    {
        InAction = true;
        IsTakingHit = true;

        var dispVec = attacker.transform.position - transform.position;
        dispVec.y = 0;
        transform.rotation = Quaternion.LookRotation(dispVec);

        OnGoHit?.Invoke(attacker);

        //动画过度函数：占原动画20%时，过度到下一个动画
        animator.CrossFade("SwordImpact", 0.2f);
        //当调用 animator.CrossFade("Slash", 0.2f) 时，
        //动画系统并不会立即切换到 "Slash" 状态，而是需要一帧的时间来处理过渡逻辑：
        yield return null;

        //获取下一个的动画信息(因为当前使用的是淡出动画函数，过渡到下一个)(1为动画层索引)
        var animState = animator.GetNextAnimatorStateInfo(1);

        //暂时等待相应时间后执行下一行函数
        yield return new WaitForSeconds(animState.length * 0.8f);

        OnHitComplete?.Invoke(); 
        InAction = false;
        IsTakingHit = false;
    }

    public IEnumerator PerformCounterAttack(EnemyController opponent)
    {
        InAction = true;

        InCounter = true;
        opponent.Fighter.InCounter = true;
        opponent.ChangeState(EnemyStates.Dead);

        // 让敌人受到攻击,敌人和玩家能够面对面
        var dispVec = opponent.transform.position - transform.position;
        dispVec.y = 0f;
        transform.rotation = Quaternion.LookRotation(dispVec);
        opponent.transform.rotation = Quaternion.LookRotation(-dispVec);

        //动画过度函数：占原动画20%时，过度到下一个动画
        animator.CrossFade("CounterAttack", 0.2f);
        opponent.Animator.CrossFade("CounterAttackVictim", 0.2f);

        // 设定为距离玩家1m远
        var targetPos = opponent.transform.position - dispVec.normalized * 1f;
                
        //当调用 animator.CrossFade("Slash", 0.2f) 时，
        //动画系统并不会立即切换到 "Slash" 状态，而是需要一帧的时间来处理过渡逻辑：
        yield return null;

        //获取下一个的动画信息(因为当前使用的是淡出动画函数，过渡到下一个)(1为动画层索引)
        var animState = animator.GetNextAnimatorStateInfo(1);

        //暂时等待相应时间后执行下一行函数
        float timer = 0f;
        while (timer <= animState.length)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 5 * Time.deltaTime);

            yield return null;

            timer += Time.deltaTime;
        }

        InCounter = false;
        opponent.Fighter.InCounter = false;

        InAction = false;
    }

    void EnableHitBox(AttackData attack)
    {
        switch (attack.HitBoxToUse)
        {
            case AttackHitbox.LeftHand:
                leftHandCollider.enabled = true;              
                break;
            case AttackHitbox.RightHand:
                rightHandCollider.enabled = true;
                break;
            case AttackHitbox.LeftFoot:
                leftFootCollider.enabled = true;
                break;
            case AttackHitbox.RightFoot:
                rightFootCollider.enabled = true;
                Debug.Log("踢");
                break;
            case AttackHitbox.Sword:
                swordCollider.enabled = true;
                Debug.Log("砍");
                break;
            default:
                break;
        }
    }

    void DisableAllHitBoxes()
    {
        if (swordCollider != null) swordCollider.enabled = false;

        if (leftHandCollider != null) leftHandCollider.enabled = false;

        if (rightHandCollider != null) rightHandCollider.enabled = false;

        if (leftFootCollider != null) leftFootCollider.enabled = false;

        if (rightFootCollider != null) rightFootCollider.enabled = false;
    }

    public List<AttackData> Attacks => attacks;

    public bool IsCounterable => AttackState == AttackStates.Windup && comboCount == 0;
}
