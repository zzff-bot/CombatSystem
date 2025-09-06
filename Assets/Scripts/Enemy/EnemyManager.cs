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
        // 判断是否存在当前这个敌人的实例，如果不存在就将其添加进列表中
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

        //如果当前所有攻击范围内的敌人都不在攻击状态,则选择一名战斗待机时间最长的敌人进行攻击
        if(!enemiesInRange.Any(e => e.IsInState(EnemyStates.Attack))){
            if (notAttackTimer > 0 )
            {
                notAttackTimer -= Time.deltaTime;
            }

            if (notAttackTimer <= 0)
            {
                // 攻击玩家
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
        //OrderByDescending():从大到小排序，FirstOrDefault():取首个元素
        return enemiesInRange.OrderByDescending(e => e.CombatMovementTimer).FirstOrDefault(e => e.Target != null);
    }

    public EnemyController GetAttackingEnemy()
    {
        // FirstOrDefault：若是没有敌人正在执行攻击，将返回null
        return enemiesInRange.FirstOrDefault(e => e.IsInState(EnemyStates.Attack));
    }
}
