using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine<T>
{
    public State<T> CurrenState { get; private set; }

    T _owner;
    public StateMachine(T owner)
    {
        _owner = owner;
    }

    //切换当前状态
    public void ChangeState(State<T> newState)
    {
        //? 运算符的作用是简化 null 检查,避免空引用
        CurrenState?.Exit();
        CurrenState = newState;
        CurrenState.Enter(_owner);
    }

    public void Execute()
    {
        CurrenState?.Execute();
    }
}
