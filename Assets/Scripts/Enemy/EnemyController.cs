using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates { Idle,CombatMovement, Attack, RetreeArterAttack, Dead}

public class EnemyController : MonoBehaviour
{
    [field:SerializeField]public float Fov { get; private set; } = 180f;

    public List<MeeleFighter> TargetsInRange { get; set; } = new List<MeeleFighter>();
    public MeeleFighter Target { get; set; }
    public float CombatMovementTimer { get; set; } = 0f;

    public StateMachine<EnemyController> StateMachine { get; private set; }

    Dictionary<EnemyStates, State<EnemyController>> stateDict;

    public NavMeshAgent NavAgent { get; private set; }
    public CharacterController CharacterController { get; private set; }
    public Animator Animator { get; private set; }
    public MeeleFighter Fighter { get; private set; }
    public VisionSensor VisionSensor { get; set; }



    private void Start()
    {
        NavAgent = GetComponent<NavMeshAgent>();
        Animator = GetComponent<Animator>();
        Fighter = GetComponent<MeeleFighter>();
        CharacterController = GetComponent<CharacterController>();

        //初始化字典
        stateDict = new Dictionary<EnemyStates, State<EnemyController>>();
        stateDict[EnemyStates.Idle] = GetComponent<IdelState>();
        stateDict[EnemyStates.CombatMovement] = GetComponent<CombatMovementState>();
        stateDict[EnemyStates.Attack] = GetComponent<AttackState>();
        stateDict[EnemyStates.RetreeArterAttack] = GetComponent<RetreeArterAttackState>();
        stateDict[EnemyStates.Dead] = GetComponent<DeadState>();

        StateMachine = new StateMachine<EnemyController>(this);
        StateMachine.ChangeState(stateDict[EnemyStates.Idle]);
    }

    // 封装一个转换敌人攻击状态的函数
    public void ChangeState(EnemyStates state)
    {
        StateMachine.ChangeState(stateDict[state]);
    }

    // 封装一个确认敌人是否处于我们想要的状态，返回布尔类型
    public bool IsInState(EnemyStates state)
    {
        return StateMachine.CurrenState == stateDict[state];
    }

    Vector3 prevPos;
    private void Update()
    {
        StateMachine.Execute();
        #region 通过记录移动方向的分量，通过动画混合树，实现动画效果
        // 1.得到上一帧和当前帧的位移  2.速度 = 位移 / 时间 3. 若是动画有根运动则将位移改为0
        var deltaPos = Animator.applyRootMotion? Vector3.zero : transform.position - prevPos;
        var velocity = deltaPos / Time.deltaTime;

        // 计算向前的速度：通过向量点积计算 “敌人的移动速度在自身正前方方向上的投影”。
        float fowardSpeed = Vector3.Dot(velocity, transform.forward);
        Animator.SetFloat("fowardSpeed", fowardSpeed / NavAgent.speed, 0.2f, Time.deltaTime);

        // 计算测方向的速度：1.计算自身与移动方向的夹角
        // 2.通过angle * Mathf.Deg2Rad将角度转换为弧度，通过sin算出侧移比例(-1 - 1)，再传给动画器
        float angle = Vector3.SignedAngle(transform.forward, velocity, Vector3.up);
        float strafeSpeed = Mathf.Sin(angle * Mathf.Deg2Rad);
        Animator.SetFloat("strafeSpeed", strafeSpeed, 0.2f, Time.deltaTime);

        prevPos = transform.position;
        #endregion
    }
}
