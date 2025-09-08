using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    public EnemyController targetEnemy;
    public EnemyController TargetEnemy
    {
        get => targetEnemy;
        set
        {
            targetEnemy = value;

            if (targetEnemy == null)
                CombatMode = false;
        }
    }

    bool combatMode;
    public bool CombatMode
    {
        get => combatMode;
        set
        {
            combatMode = value;

            if (targetEnemy == null)
                combatMode = false;

            animator.SetBool("combatMode", combatMode);
        }
    }

    MeeleFighter meeleFighter;
    Animator animator;
    CameraController cam;

    private void Awake()
    {
        meeleFighter = GetComponent<MeeleFighter>();
        animator = GetComponent<Animator>();
        cam = Camera.main.GetComponent<CameraController>();
    }

    private void Start()
    {
        meeleFighter.OnGoHit += (MeeleFighter attacker) =>
        {
            if (CombatMode && attacker != TargetEnemy.Fighter)
                TargetEnemy = attacker.GetComponent<EnemyController>();
        };
    }

    private void Update()
    {
        if (Input.GetButtonDown("Attack") && !meeleFighter.IsTakingHit)
        {
            var enemy = EnemyManager.i.GetAttackingEnemy();
            if(enemy != null && enemy.Fighter.IsCounterable && !meeleFighter.InAction)
            {
                StartCoroutine(meeleFighter.PerformCounterAttack(enemy));
            }
            else
            {
                //攻击玩家输入方向上最近的敌人
                var enemyToAttack = EnemyManager.i.GetClosesEnenmyToDirection(PlayerController.i.GetIntentDirection());

                meeleFighter.TryToAttack(enemyToAttack?.Fighter);

                CombatMode = true;
            }
        }

        if (Input.GetButtonDown("LockOn") || JoystickHelper.i.GetAxisDown("LockOnTrigger"))
        {
            CombatMode = !CombatMode;
        }
    }

    private void OnAnimatorMove()
    {
        if(!meeleFighter.InAction)
        transform.position += animator.deltaPosition;
        
        transform.rotation *= animator.deltaRotation;
    }

    public Vector3 GetTargetingDir()
    {
        if (!combatMode)
        {
            //获取玩家的视线方向
            var vecFromCam = transform.position - cam.transform.position;
            vecFromCam.y = 0;
            return vecFromCam.normalized;
        }
        else
        {
            return transform.forward;
        }
        
    }
}
