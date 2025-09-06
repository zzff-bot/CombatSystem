using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackStates{ Idle, Windup, Imapct, Cooldown}


public class MeeleFighter : MonoBehaviour
{
    [SerializeField] List<AttackData> attacks;
    [SerializeField] GameObject sword;

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

    public void TryToAttack()
    {
        if (!InAction)
        {
            StartCoroutine(Attack());
        }
        else if (AttackState == AttackStates.Imapct || AttackState == AttackStates.Cooldown)
        {
            doCombo = true;
        }
    }

    IEnumerator Attack()
    {
        InAction = true;

        AttackState = AttackStates.Windup;

        //动画过度函数：占原动画20%时，过度到下一个动画
        animator.CrossFade(attacks[comboCount].AnimName, 0.2f);
        //当调用 animator.CrossFade("Slash", 0.2f) 时，
        //动画系统并不会立即切换到 "Slash" 状态，而是需要一帧的时间来处理过渡逻辑：
        yield return null;

        //获取下一个的动画信息(因为当前使用的是淡出动画函数，过渡到下一个)
        var animState = animator.GetNextAnimatorStateInfo(1);

        float timer = 0f;
        while (timer <= animState.length)
        {
            timer += Time.deltaTime;

            float normalizedTime = timer / animState.length;

            if (AttackState == AttackStates.Windup)
            {
                if (InCounter) break;

                if(normalizedTime >= attacks[comboCount].ImpactStartTime)
                {
                    AttackState = AttackStates.Imapct;
                    //打开触发器
                    EnableHitBox(attacks[comboCount]);
                }
            }else if (AttackState == AttackStates.Imapct)
            {
                if (normalizedTime >= attacks[comboCount].ImpactEndTime)
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
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Hitbox" && !InAction)
        {
            Debug.Log("角色受伤");
            StartCoroutine(PlayerHitReaction());
        }
    }

    IEnumerator PlayerHitReaction()
    {
        InAction = true;
        //动画过度函数：占原动画20%时，过度到下一个动画
        animator.CrossFade("SwordImpact", 0.2f);
        //当调用 animator.CrossFade("Slash", 0.2f) 时，
        //动画系统并不会立即切换到 "Slash" 状态，而是需要一帧的时间来处理过渡逻辑：
        yield return null;

        //获取下一个的动画信息(因为当前使用的是淡出动画函数，过渡到下一个)(1为动画层索引)
        var animState = animator.GetNextAnimatorStateInfo(1);

        //暂时等待相应时间后执行下一行函数
        yield return new WaitForSeconds(animState.length * 0.8f);

        InAction = false;
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
