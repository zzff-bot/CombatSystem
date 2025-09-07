using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField] float moveSpeed = 5f;  // 移动速度
    [SerializeField] float rotationSpeed = 500f;  // 旋转速度

    [SerializeField] float groundCheckRadius = 0.2f;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] LayerMask groundLayer;

    bool isGround;

    float ySpeed;
    Quaternion targetRotation;

    CameraController cameraController;

    CharacterController characterController;

    Animator animator;
   
    MeeleFighter meeleFighter;

    CombatController combatController;

    private void Awake()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        meeleFighter = GetComponent<MeeleFighter>();
        combatController = GetComponent<CombatController>();
    }

    void Update()
    {
        if (meeleFighter.InAction)
        {
            // 进行反击后，将当前位置设置为旋转位置，避免反击后又出现一个突然的旋转
            targetRotation = transform.rotation;

            //因为虽然没有移动但是动画moveAount值还不为0
            animator.SetFloat("fowardSpeed", 0);
            return;
        }

        //获取角色水平和垂直位置
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        //通过Mathf.Clamp01()将值限制在0-1之间 
        float moveAmount = Mathf.Clamp01(Mathf.Abs(h) + Mathf.Abs(v));

        //角色的移动方向
        var moveInput = (new Vector3(h , 0, v)).normalized;

        var moveDir = cameraController.PlanarRotation * moveInput;

        GroundCheck();
        //Debug.Log("isGround = " + isGround);
        if (isGround)
        {
            ySpeed = -0.5f;
        }
        else
        {
            ySpeed += Physics.gravity.y * Time.deltaTime;
        }

        var velocity = moveDir * moveSpeed;
        

        

        if (combatController.CombatMode)
        {
            velocity /= 4f;

            // 进入战斗模式时面向敌人
            var targetVec = combatController.TargetEnemy.transform.position - transform.position;
            targetVec.y = 0;

            if (moveAmount > 0)
            {
                //记录角色旋转位置
                targetRotation = Quaternion.LookRotation(targetVec);
                //实现角色原地平滑旋转
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            //分割速度为向前速度 和 侧向速度
            // 计算向前的速度：通过向量点积计算 “敌人的移动速度在自身正前方方向上的投影”。
            float fowardSpeed = Vector3.Dot(velocity, transform.forward);
            animator.SetFloat("fowardSpeed", fowardSpeed / moveSpeed, 0.2f, Time.deltaTime);

            // 计算测方向的速度：1.计算自身与移动方向的夹角
            // 2.通过angle * Mathf.Deg2Rad将角度转换为弧度，通过sin算出侧移比例(-1 - 1)，再传给动画器
            float angle = Vector3.SignedAngle(transform.forward, velocity, Vector3.up);
            float strafeSpeed = Mathf.Sin(angle * Mathf.Deg2Rad);
            animator.SetFloat("strafeSpeed", strafeSpeed, 0.2f, Time.deltaTime);
        }
        else
        {
            if (moveAmount > 0)
            {
                //记录角色旋转位置
                targetRotation = Quaternion.LookRotation(moveDir);
            }

            //实现角色原地平滑旋转
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            //增加阻尼和时间
            animator.SetFloat("fowardSpeed", moveAmount, 0.2f, Time.deltaTime);
        }

        //角色位置(通过角色控制器)
        velocity.y = ySpeed;
        characterController.Move(velocity * Time.deltaTime);
    }

    //地面检测，世界坐标，在角色脚下，检测半径，检测图层
    void GroundCheck()
    {
        isGround = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);
    }

    //脚本所挂载的物体被选中时才会执行：实现可视化
    private void OnDrawGizmosSelected()
    {
        //设置绘制图形的颜色
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        //绘制一个球形，位置和半径与检测大小相同
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
    }

}
