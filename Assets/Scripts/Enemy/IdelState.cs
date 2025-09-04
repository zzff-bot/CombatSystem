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
        Debug.Log("Excuting Idle State");

        if (Input.GetKeyDown(KeyCode.T))
        {
            enemy.ChangeState(EnemyStates.Chase);
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting Idle State");
    }
}
