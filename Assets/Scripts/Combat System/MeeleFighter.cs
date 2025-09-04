using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeeleFighter : MonoBehaviour
{
    Animator animator;

    //��������Ϊһ�����ԣ�����set��Ϊ˽�ˣ�ʹ�����಻�����ı�(����Ҫ�Դ�д��ĸ��ͷ)
    public bool InAction { get; private set; } = false;  //�Ƿ����ڹ���

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void TryToAttack()
    {
        if (!InAction)
        {
            StartCoroutine(Attack());
        }
    }

    IEnumerator Attack()
    {
        InAction = true;
        //�������Ⱥ�����ռԭ����20%ʱ�����ȵ���һ������
        animator.CrossFade("Slash", 0.2f);
        //������ animator.CrossFade("Slash", 0.2f) ʱ��
        //����ϵͳ�����������л��� "Slash" ״̬��������Ҫһ֡��ʱ������������߼���
        yield return null;

        //��ȡ��һ���Ķ�����Ϣ(��Ϊ��ǰʹ�õ��ǵ����������������ɵ���һ��)
        var animState = animator.GetNextAnimatorStateInfo(1);

        //��ʱ�ȴ���Ӧʱ���ִ����һ�к���
        yield return new WaitForSeconds(animState.length);

        InAction = false;
    }

}
