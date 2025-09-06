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

    //��������Ϊһ�����ԣ�����set��Ϊ˽�ˣ�ʹ�����಻�����ı�(����Ҫ�Դ�д��ĸ��ͷ)
    public bool InAction { get; private set; } = false;  //�Ƿ����ڹ���

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

        //�������Ⱥ�����ռԭ����20%ʱ�����ȵ���һ������
        animator.CrossFade(attacks[comboCount].AnimName, 0.2f);
        //������ animator.CrossFade("Slash", 0.2f) ʱ��
        //����ϵͳ�����������л��� "Slash" ״̬��������Ҫһ֡��ʱ������������߼���
        yield return null;

        //��ȡ��һ���Ķ�����Ϣ(��Ϊ��ǰʹ�õ��ǵ����������������ɵ���һ��)
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
                    //�򿪴�����
                    EnableHitBox(attacks[comboCount]);
                }
            }else if (AttackState == AttackStates.Imapct)
            {
                if (normalizedTime >= attacks[comboCount].ImpactEndTime)
                {
                    AttackState = AttackStates.Cooldown;
                    //�رմ�����
                    DisableAllHitBoxes();
                }
            }
            else if (AttackState == AttackStates.Cooldown)
            {
                if (doCombo)
                {
                    doCombo = false;
                    //������������ == �б�������ʱ��ģΪ0
                    comboCount = (comboCount + 1) % attacks.Count;

                    StartCoroutine(Attack());

                    //�˳�Э��
                    yield break;
                }
            }

            //��ʱ�ȴ�һ֡��ִ����һ�к���
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
            Debug.Log("��ɫ����");
            StartCoroutine(PlayerHitReaction());
        }
    }

    IEnumerator PlayerHitReaction()
    {
        InAction = true;
        //�������Ⱥ�����ռԭ����20%ʱ�����ȵ���һ������
        animator.CrossFade("SwordImpact", 0.2f);
        //������ animator.CrossFade("Slash", 0.2f) ʱ��
        //����ϵͳ�����������л��� "Slash" ״̬��������Ҫһ֡��ʱ������������߼���
        yield return null;

        //��ȡ��һ���Ķ�����Ϣ(��Ϊ��ǰʹ�õ��ǵ����������������ɵ���һ��)(1Ϊ����������)
        var animState = animator.GetNextAnimatorStateInfo(1);

        //��ʱ�ȴ���Ӧʱ���ִ����һ�к���
        yield return new WaitForSeconds(animState.length * 0.8f);

        InAction = false;
    }

    public IEnumerator PerformCounterAttack(EnemyController opponent)
    {
        InAction = true;

        InCounter = true;
        opponent.Fighter.InCounter = true;
        opponent.ChangeState(EnemyStates.Dead);

        // �õ����ܵ�����,���˺�����ܹ������
        var dispVec = opponent.transform.position - transform.position;
        dispVec.y = 0f;
        transform.rotation = Quaternion.LookRotation(dispVec);
        opponent.transform.rotation = Quaternion.LookRotation(-dispVec);

        //�������Ⱥ�����ռԭ����20%ʱ�����ȵ���һ������
        animator.CrossFade("CounterAttack", 0.2f);
        opponent.Animator.CrossFade("CounterAttackVictim", 0.2f);

        // �趨Ϊ�������1mԶ
        var targetPos = opponent.transform.position - dispVec.normalized * 1f;
                
        //������ animator.CrossFade("Slash", 0.2f) ʱ��
        //����ϵͳ�����������л��� "Slash" ״̬��������Ҫһ֡��ʱ������������߼���
        yield return null;

        //��ȡ��һ���Ķ�����Ϣ(��Ϊ��ǰʹ�õ��ǵ����������������ɵ���һ��)(1Ϊ����������)
        var animState = animator.GetNextAnimatorStateInfo(1);

        //��ʱ�ȴ���Ӧʱ���ִ����һ�к���
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
                Debug.Log("��");
                break;
            case AttackHitbox.Sword:
                swordCollider.enabled = true;
                Debug.Log("��");
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
