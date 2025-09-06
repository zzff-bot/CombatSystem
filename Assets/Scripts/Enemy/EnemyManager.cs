using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] Vector2 timeRangeBetweenAttacks = new Vector2(1, 4);
    public static EnemyManager i { get; private set; }

    List<EnemyController> enemiesInRange = new List<EnemyController>();
    float notAttackTimer = 2f;

    private void Awake()
    {
        i = this;
    }

    public void AddEnemyInRange(EnemyController enemy)
    {
        // �ж��Ƿ���ڵ�ǰ������˵�ʵ������������ھͽ�����ӽ��б���
        if (!enemiesInRange.Contains(enemy))
        {
            enemiesInRange.Add(enemy);
        }       
    }

    public void RemoveEnemyInRange(EnemyController enemy)
    {
        enemiesInRange.Remove(enemy);
    }

    private void Update()
    {
        if (enemiesInRange.Count == 0)
        {
            return;
        }

        //�����ǰ���й�����Χ�ڵĵ��˶����ڹ���״̬,��ѡ��һ��ս������ʱ����ĵ��˽��й���
        if(!enemiesInRange.Any(e => e.IsInState(EnemyStates.Attack))){
            if (notAttackTimer > 0 )
            {
                notAttackTimer -= Time.deltaTime;
            }

            if (notAttackTimer <= 0)
            {
                // �������
                var attackingEnemy = SelectEnemyForAttack();

                if (attackingEnemy != null)
                {
                    attackingEnemy.ChangeState(EnemyStates.Attack);
                    notAttackTimer = Random.Range(timeRangeBetweenAttacks.x, timeRangeBetweenAttacks.y);
                }
            }
        }
    }

    EnemyController SelectEnemyForAttack()
    {
        //OrderByDescending():�Ӵ�С����FirstOrDefault():ȡ�׸�Ԫ��
        return enemiesInRange.OrderByDescending(e => e.CombatMovementTimer).FirstOrDefault(e => e.Target != null);
    }

    public EnemyController GetAttackingEnemy()
    {
        // FirstOrDefault������û�е�������ִ�й�����������null
        return enemiesInRange.FirstOrDefault(e => e.IsInState(EnemyStates.Attack));
    }
}
