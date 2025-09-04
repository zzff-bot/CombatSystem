using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeeleFighter : MonoBehaviour
{
    Animator animator;

    //将其设置为一个属性，并将set设为私人，使其他类不能随便改变(属性要以大写字母开头)
    public bool InAction { get; private set; } = false;  //是否正在攻击

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
        //动画过度函数：占原动画20%时，过度到下一个动画
        animator.CrossFade("Slash", 0.2f);
        //当调用 animator.CrossFade("Slash", 0.2f) 时，
        //动画系统并不会立即切换到 "Slash" 状态，而是需要一帧的时间来处理过渡逻辑：
        yield return null;

        //获取下一个的动画信息(因为当前使用的是淡出动画函数，过渡到下一个)
        var animState = animator.GetNextAnimatorStateInfo(1);

        //暂时等待相应时间后执行下一行函数
        yield return new WaitForSeconds(animState.length);

        InAction = false;
    }

}
