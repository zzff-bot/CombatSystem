using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GettingHitState : State<EnemyController>
{
    [SerializeField] float stunnTime = 0.5f;

    EnemyController enemy;

    public override void Enter(EnemyController owner)
    {
        StopAllCoroutines();

        enemy = owner;
        enemy.Fighter.OnHitComplete += () => StartCoroutine(GoToCombatMovement());
    }

    IEnumerator GoToCombatMovement()
    {
        // 晕眩一下会儿后，回到战斗退后状态
        yield return new WaitForSeconds(stunnTime);
        enemy.ChangeState(EnemyStates.CombatMovement);
    }
}
