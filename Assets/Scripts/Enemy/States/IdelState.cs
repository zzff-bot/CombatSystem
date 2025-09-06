using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdelState : State<EnemyController>
{
    EnemyController enemy;

    public override void Enter(EnemyController owner)
    {
        enemy = owner;
        Debug.Log("Enter Idle State");
    }

    public override void Execute()
    {
        foreach (var target in enemy.TargetsInRange)
        {
            var vecToTarget = target.transform.position - transform.position;

            //���������ǰ�ķ��������λ�õĽǶ�
            float angle = Vector3.Angle(transform.forward, vecToTarget);

            //�����ӽ�180��/ 2���������ڵ�����ǰ����������90���ڣ������ڷ�Χ��
            if (angle <= enemy.Fov / 2)
            {
                enemy.Target = target;
                enemy.ChangeState(EnemyStates.CombatMovement);
                break;
            }
        }
    }

    public override void Exit()
    {
        
    }
}
