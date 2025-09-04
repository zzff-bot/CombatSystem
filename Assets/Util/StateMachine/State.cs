using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State<T> : MonoBehaviour
{
    //Enter函数获取状态的所有者,设定为虚函数，让其子类能够覆盖他
    public virtual void Enter(T owner){ }

    //执行状态逻辑函数
    public virtual void Execute(){ }

    //退出函数
    public virtual void Exit(){ }


}
