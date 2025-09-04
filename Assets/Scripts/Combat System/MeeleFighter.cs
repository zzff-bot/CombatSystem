using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackState { Idle, Windup, Imapct, Cooldown}


public class MeeleFighter : MonoBehaviour
{
    [SerializeField] List<AttackData> attacks;
    [SerializeField] GameObject sword;

    BoxCollider swordCollider;
    SphereCollider leftHandCollider, rightHandCollider, leftFootCollider, rightFootCollider;

    Animator animator;

    AttackState attackState;

    bool doCombo;
    int comboCount = 0;

    //��������Ϊһ�����ԣ�����set��Ϊ˽�ˣ�ʹ�����಻�����ı�(����Ҫ�Դ�д��ĸ��ͷ)
    public bool InAction { get; private set; } = false;  //�Ƿ����ڹ���

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
        else if (attackState == AttackState.Imapct || attackState == AttackState.Cooldown)
        {
            doCombo = true;
        }
    }

    IEnumerator Attack()
    {
        InAction = true;

        attackState = AttackState.Windup;

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

            if (attackState == AttackState.Windup)
            {
                if(normalizedTime >= attacks[comboCount].ImpactStartTime)
                {
                    attackState = AttackState.Imapct;
                    //�򿪴�����
                    EnableHitBox(attacks[comboCount]);
                }
            }else if (attackState == AttackState.Imapct)
            {
                if (normalizedTime >= attacks[comboCount].ImpactEndTime)
                {
                    attackState = AttackState.Cooldown;
                    //�رմ�����
                    DisableAllHitBoxes();
                }
            }
            else if (attackState == AttackState.Cooldown)
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

        attackState = AttackState.Idle;
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
        swordCollider.enabled = false;
        leftHandCollider.enabled = false;
        rightHandCollider.enabled = false;
        leftFootCollider.enabled = false;
        rightFootCollider.enabled = false;
    }

}
