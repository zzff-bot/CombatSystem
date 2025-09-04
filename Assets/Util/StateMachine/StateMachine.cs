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

    //�л���ǰ״̬
    public void ChangeState(State<T> newState)
    {
        //? ������������Ǽ� null ���,���������
        CurrenState?.Exit();
        CurrenState = newState;
        CurrenState.Enter(_owner);
    }

    public void Execute()
    {
        CurrenState?.Execute();
    }
}
