using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    MeeleFighter meeleFighter;

    private void Awake()
    {
        meeleFighter = GetComponent<MeeleFighter>();    
    }

    private void Update()
    {
        if (Input.GetButtonDown("Attack"))
        {
            meeleFighter.TryToAttack();
        }
    }

}
