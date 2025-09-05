using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates { Idle,CombatMovement}

public class EnemyController : MonoBehaviour
{
    [field:SerializeField]public float Fov { get; private set; } = 180f;

    public List<MeeleFighter> TargetsInRange { get; set; } = new List<MeeleFighter>();
    public MeeleFighter Target { get; set; }

    public StateMachine<EnemyController> StateMachine { get; private set; }

    Dictionary<EnemyStates, State<EnemyController>> stateDict;

    public NavMeshAgent NavAgent { get; private set; }
    public Animator Animator { get; private set; }



    private void Start()
    {
        NavAgent = GetComponent<NavMeshAgent>();
        Animator = GetComponent<Animator>();

        stateDict = new Dictionary<EnemyStates, State<EnemyController>>();
        stateDict[EnemyStates.Idle] = GetComponent<IdelState>();
        stateDict[EnemyStates.CombatMovement] = GetComponent<CombatMovementState>();

        StateMachine = new StateMachine<EnemyController>(this);
        StateMachine.ChangeState(stateDict[EnemyStates.Idle]);
    }

    public void ChangeState(EnemyStates state)
    {
        StateMachine.ChangeState(stateDict[state]);
    }

    Vector3 prevPos;
    private void Update()
    {
        StateMachine.Execute();

        // 1.�õ���һ֡�͵�ǰ֡��λ��  2.�ٶ� = λ�� / ʱ��
        var deltaPos = transform.position - prevPos;
        var velocity = deltaPos / Time.deltaTime;

        // ������ǰ���ٶȣ�ͨ������������� �����˵��ƶ��ٶ���������ǰ�������ϵ�ͶӰ����
        float fowardSpeed = Vector3.Dot(velocity, transform.forward);
        Animator.SetFloat("fowardSpeed", fowardSpeed / NavAgent.speed, 0.2f, Time.deltaTime);

        // ����ⷽ����ٶȣ�1.�����������ƶ�����ļн�
        // 2.ͨ��angle * Mathf.Deg2Rad���Ƕ�ת��Ϊ���ȣ�ͨ��sin������Ʊ���(-1 - 1)���ٴ���������
        float angle = Vector3.SignedAngle(transform.forward, velocity, Vector3.up);
        float strafeSpeed = Mathf.Sin(angle * Mathf.Deg2Rad);
        Animator.SetFloat("strafeSpeed", strafeSpeed, 0.2f, Time.deltaTime);

        prevPos = transform.position;
    }
}
