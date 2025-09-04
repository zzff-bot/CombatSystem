using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State<T> : MonoBehaviour
{
    //Enter������ȡ״̬��������,�趨Ϊ�麯�������������ܹ�������
    public virtual void Enter(T owner){ }

    //ִ��״̬�߼�����
    public virtual void Execute(){ }

    //�˳�����
    public virtual void Exit(){ }


}
