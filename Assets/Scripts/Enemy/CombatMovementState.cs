using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AICombatStates { Idle,Chase,Circling}

public class CombatMovementState : State<EnemyController>
{
    [SerializeField] float circlingSpeed = 20f;
    [SerializeField] float distanceToStand = 3f;
    [SerializeField] float adjustDistanceThreshould = 1f;
    [SerializeField] Vector2 idleTimeRange = new Vector2(2, 5); //���ս���ռ�ʱ�䷶Χ
    [SerializeField] Vector2 circlingTimeRange = new Vector2(3, 6); //���ս���ռ�ʱ�䷶Χ

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

            Debug.Log("��ʼ��ת");

            var vecToTarget = enemy.transform.position - enemy.Target.transform.position;

            var rotatePos = Quaternion.Euler(0, circlingSpeed * circlingDir * Time.deltaTime, 0) * vecToTarget;

            //�õ�����ת����ʼ�տ������
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

        //���ԭ����enemy.NavAgent.SetDestination()·����
        //�����м�һ�������ϰ��ԭ���ĵ��������ǿ������ִ�����ɵ�·����һֱ������ң�
        //������enemy.NavAgent.Move()�Ͳ�����ԭ������·��Լ�������������ƶ�
        enemy.NavAgent.ResetPath();
        timer = Random.Range(circlingTimeRange.x, circlingTimeRange.y);

        circlingDir = Random.Range(0, 2) == 0 ? 1 : -1;

    }

    public override void Exit()
    {
        Debug.Log("Exiting Cahse State");
    }
}
