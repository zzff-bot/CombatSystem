using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AICombatStates { Idle,Chase,Circling}

public class CombatMovementState : State<EnemyController>
{
    [SerializeField] float circlingSpeed = 20f;
    [SerializeField] float distanceToStand = 3f;
    [SerializeField] float adjustDistanceThreshould = 1f;
    [SerializeField] Vector2 idleTimeRange = new Vector2(2, 5); //随机战斗空间时间范围
    [SerializeField] Vector2 circlingTimeRange = new Vector2(3, 6); //随机战斗空间时间范围

    float timer = 0f;

    int circlingDir = 1;

    AICombatStates state;

    EnemyController enemy;

    public override void Enter(EnemyController owner)
    {
        Debug.Log("Enter Cahse State");
        enemy = owner;

        enemy.NavAgent.stoppingDistance = distanceToStand;
    }

    public override void Execute()
    {
        if (Vector3.Distance(enemy.Target.transform.position,enemy.transform.position) > distanceToStand + adjustDistanceThreshould)
        {
            StartChase();
        }

        if (state == AICombatStates.Idle)
        {
            if (timer <= 0)
            {
                if(Random.Range(0, 2) == 0)
                {
                    StartIdle();
                }
                else
                {
                    StartCircling();
                }
            }
        }
        else if (state == AICombatStates.Chase)
        {
            if (Vector3.Distance(enemy.Target.transform.position, enemy.transform.position) <= distanceToStand + 0.03f)
            {
                StartIdle();
                return;
            }

            enemy.NavAgent.SetDestination(enemy.Target.transform.position);
        }
        else if (state == AICombatStates.Circling)
        {
            if (timer <= 0)
            {
                StartIdle();
                return;
            }

            Debug.Log("开始旋转");

            var vecToTarget = enemy.transform.position - enemy.Target.transform.position;

            var rotatePos = Quaternion.Euler(0, circlingSpeed * circlingDir * Time.deltaTime, 0) * vecToTarget;

            //让敌人旋转，并始终看向玩家
            enemy.NavAgent.Move(rotatePos - vecToTarget);
            enemy.transform.rotation = Quaternion.LookRotation(-rotatePos);
        }

        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
                
    }

    void StartChase()
    {
        state = AICombatStates.Chase;
        enemy.Animator.SetBool("combatMode", false);
    }

    void StartIdle()
    {
        state = AICombatStates.Idle;
        timer = Random.Range(idleTimeRange.x,idleTimeRange.y);
        
        enemy.Animator.SetBool("combatMode", true);
    }

    void StartCircling()
    {
        state = AICombatStates.Circling;

        //清除原本的enemy.NavAgent.SetDestination()路径，
        //否则中间一旦产生障碍物，原本的导航网格会强制让他执行生成的路径，一直看向玩家，
        //消除后，enemy.NavAgent.Move()就不再受原本导航路径约束，可以自由移动
        enemy.NavAgent.ResetPath();
        timer = Random.Range(circlingTimeRange.x, circlingTimeRange.y);

        circlingDir = Random.Range(0, 2) == 0 ? 1 : -1;

    }

    public override void Exit()
    {
        Debug.Log("Exiting Cahse State");
    }
}
