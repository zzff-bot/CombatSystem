using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : State<EnemyController>
{
    public override void Enter(EnemyController owner)
    {
        Debug.Log("Enter Cahse State");
    }

    public override void Execute()
    {
        Debug.Log("Excuting Cahse State");
    }

    public override void Exit()
    {
        Debug.Log("Exiting Cahse State");
    }
}
