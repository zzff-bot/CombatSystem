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

            //计算敌人面前的方向与玩家位置的角度
            float angle = Vector3.Angle(transform.forward, vecToTarget);

            //敌人视角180°/ 2，这样当在敌人面前的左右两边90°内，都是在范围内
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
