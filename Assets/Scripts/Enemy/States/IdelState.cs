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

        enemy.Animator.SetBool("combatMode", false);
    }

    public override void Execute()
    {    
        enemy.Target = enemy.FindTarget();
        if (enemy.Target != null)
        {
            enemy.AlertNearbyEnemies();
            enemy.ChangeState(EnemyStates.CombatMovement);
        }
    }

    public override void Exit()
    {
        
    }
}
