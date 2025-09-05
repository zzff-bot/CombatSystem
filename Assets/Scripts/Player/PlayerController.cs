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

    private void Awake()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        meeleFighter = GetComponent<MeeleFighter>();
    }

    void Update()
    {
        if (meeleFighter.InAction)
        {
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
        velocity.y = ySpeed;

        //角色位置(通过角色控制器)
        characterController.Move(velocity * Time.deltaTime);

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
