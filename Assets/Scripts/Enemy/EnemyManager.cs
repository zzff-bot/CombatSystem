using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] Vector2 timeRangeBetweenAttacks = new Vector2(1, 4);
    [SerializeField] CombatController player;

    [field: SerializeField] public LayerMask EnemyLayer { get; private set; }
    public static EnemyManager i { get; private set; }

    List<EnemyController> enemiesInRange = new List<EnemyController>();
    float notAttackTimer = 2f;

    float timer = 0;

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

        if (enemy == player.targetEnemy)
        {
            enemy.MeshHighlighter?.HighlightMesh(false);
            player.targetEnemy = GetClosesEnenmyToDirection(player.GetTargetingDir());
            player.targetEnemy?.MeshHighlighter?.HighlightMesh(true);
        }
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

        // 检查是否锁定敌人，使其发光，并且取消上个敌人的发光效果
        if (timer >= 0.1f)
        {
            timer = 0f;
            var closetEnenmy = GetClosesEnenmyToDirection(player.GetTargetingDir());

            if (closetEnenmy != null && closetEnenmy != player.TargetEnemy)
            {
                var prevEnenmy = player.TargetEnemy;
                player.TargetEnemy = closetEnenmy;

                player?.TargetEnemy.MeshHighlighter.HighlightMesh(true);
                prevEnenmy?.MeshHighlighter.HighlightMesh(false);
            }
        }
        
        timer += Time.deltaTime;
    }

    EnemyController SelectEnemyForAttack()
    {
        //OrderByDescending():从大到小排序，FirstOrDefault():取首个元素
        return enemiesInRange.OrderByDescending(e => e.CombatMovementTimer).FirstOrDefault(e => e.Target != null && e.IsInState(EnemyStates.CombatMovement));
    }

    public EnemyController GetAttackingEnemy()
    {
        // FirstOrDefault：若是没有敌人正在执行攻击，将返回null
        return enemiesInRange.FirstOrDefault(e => e.IsInState(EnemyStates.Attack));
    }

    public EnemyController GetClosesEnenmyToDirection(Vector3 direction)
    {

        float miniDistance = Mathf.Infinity;
        EnemyController closestEnenmy = null;

        foreach (var enemy in enemiesInRange)
        {
            var vecToEnenmy = enemy.transform.position - player.transform.position;
            vecToEnenmy.y = 0;

            // 通过sin算出，敌人与玩家视线方向的距离
            float angle = Vector3.Angle(direction, vecToEnenmy);
            float distance = vecToEnenmy.magnitude * Mathf.Sin(angle * Mathf.Deg2Rad);

            if (distance < miniDistance)
            {
                miniDistance = distance;
                closestEnenmy = enemy;
            }
        }

        return closestEnenmy;
    }
}
